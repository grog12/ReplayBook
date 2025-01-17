﻿using Etirps.RiZhi;
using LiteDB;
using Rofl.Files.Models;
using Rofl.Reader.Models;
using Rofl.Settings.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Rofl.Files.Repositories
{
    public class DatabaseRepository
    {
        private readonly RiZhi _log;
        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache", "replayCache.db");
        private readonly ObservableSettings _settings;

        public DatabaseRepository(ObservableSettings settings, RiZhi log)
        {
            _settings = settings;
            _log = log;

            try
            {
                InitializeDatabase();
            }
            catch (Exception ex)
            {
                _log.Warning($"Database file is invalid, deleting and trying again... exception:{ex}");
                File.Delete(_filePath);
                InitializeDatabase();
            }
        }

        public string GetDatabasePath()
        {
            return _filePath;
        }

        public void DeleteDatabase()
        {
            File.Delete(_filePath);
        }

        private void InitializeDatabase()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_filePath)))
            {
                _ = Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            }

            using (LiteDatabase db = new LiteDatabase(_filePath))
            {
                // Create and verify file results collection
                LiteCollection<FileResult> fileResults = db.GetCollection<FileResult>("fileResults");

                _ = BsonMapper.Global.Entity<FileResult>()
                    .Id(r => r.Id)
                    .DbRef(r => r.FileInfo, "replayFileInfo")
                    .DbRef(r => r.ReplayFile, "replayFiles");

                _ = BsonMapper.Global.Entity<ReplayFileInfo>()
                    .Id(r => r.Path);

                _ = BsonMapper.Global.Entity<Player>()
                    .Id(r => r.Id);

                _ = BsonMapper.Global.Entity<ReplayFile>()
                    .Id(r => r.Location)
                    .DbRef(r => r.Players, "players");

                _ = fileResults.EnsureIndex(x => x.FileName);
                _ = fileResults.EnsureIndex(x => x.AlternativeName);
                _ = fileResults.EnsureIndex(x => x.FileSizeBytes);
                _ = fileResults.EnsureIndex(x => x.FileCreationTime);
                _ = fileResults.EnsureIndex(x => x.SearchKeywords);
            }
        }

        public void AddFileResult(FileResult result)
        {
            if (result == null) { throw new ArgumentNullException(nameof(result)); }
            if (result.ReplayFile == null) { throw new ArgumentNullException(nameof(result)); }
            if (result.FileInfo == null) { throw new ArgumentNullException(nameof(result)); }

            using (var db = new LiteDatabase(_filePath))
            {
                var fileResults = db.GetCollection<FileResult>("fileResults");
                var fileInfos = db.GetCollection<ReplayFileInfo>("replayFileInfo");
                var replayFiles = db.GetCollection<ReplayFile>("replayFiles");
                var players = db.GetCollection<Player>("players");

                // If we already have the file, do nothing
                if (fileResults.FindById(result.Id) == null)
                {
                    fileResults.Insert(result);
                }

                // Only add if it doesnt exist
                if (fileInfos.FindById(result.FileInfo.Path) == null)
                {
                    fileInfos.Insert(result.FileInfo);
                }

                if (replayFiles.FindById(result.ReplayFile.Location) == null)
                {
                    replayFiles.Insert(result.ReplayFile);
                }

                foreach (var player in result.ReplayFile.Players)
                {
                    // If the player already exists, do nothing
                    if (players.FindById(player.Id) == null)
                    {
                        players.Insert(player);
                    }
                }
            }
        }

        public void RemoveFileResult(string id)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentNullException(id); }

            using (var db = new LiteDatabase(_filePath))
            {
                var fileResults = db.GetCollection<FileResult>("fileResults");
                var fileInfos = db.GetCollection<ReplayFileInfo>("replayFileInfo");
                var replayFiles = db.GetCollection<ReplayFile>("replayFiles");
                var players = db.GetCollection<Player>("players");

                fileResults.Delete(id);

                fileInfos.Delete(id);

                replayFiles.Delete(id);

                // Rip player data is being orphaned...
            }
        }

        public FileResult GetFileResult(string id)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentNullException(id); }

            using (var db = new LiteDatabase(_filePath))
            {
                var fileResults = db.GetCollection<FileResult>("fileResults");

                var result = fileResults
                    .IncludeAll()
                    .FindById(id);

                return result;
            }
        }

        public IEnumerable<FileResult> GetReplayFiles()
        {
            using (var db = new LiteDatabase(_filePath))
            {
                var fileResults = db.GetCollection<FileResult>("fileResults");

                return fileResults.FindAll();
            }
        }

        public IReadOnlyCollection<FileResult> QueryReplayFiles(string[] keywords, SortMethod sort, int maxEntries, int skip)
        {
            if (keywords == null) { throw new ArgumentNullException(nameof(keywords)); }

            Query sortQuery;
            switch (sort)
            {
                default:
                    sortQuery = Query.All("FileCreationTime", Query.Ascending);
                    break;
                case SortMethod.DateDesc:
                    sortQuery = Query.All("FileCreationTime", Query.Descending);
                    break;
                case SortMethod.SizeAsc:
                    sortQuery = Query.All("FileSizeBytes", Query.Ascending);
                    break;
                case SortMethod.SizeDesc:
                    sortQuery = Query.All("FileSizeBytes", Query.Descending);
                    break;
                case SortMethod.NameAsc:
                    // Query either filename or alternative name
                    sortQuery = Query.All(_settings.RenameAction == RenameAction.File ? "FileName" : "AlternativeName", Query.Ascending);
                    break;
                case SortMethod.NameDesc:
                    sortQuery = Query.All(_settings.RenameAction == RenameAction.File ? "FileName" : "AlternativeName", Query.Descending);
                    break;
            }

            // Create queries trying to match keywords into search string
            List<Query> queries = new List<Query>();
            foreach (var word in keywords)
            {
                queries.Add
                (
                    Query.Where("SearchKeywords", fileKeywords => fileKeywords.AsString.Contains(word.ToUpper(CultureInfo.InvariantCulture)))
                );
            }
            
            // Comebine all keyword queries using AND keyword, then AND by sort query
            Query endQuery;
            if (queries.Any())
            {
                if (queries.Count == 1)
                {
                    endQuery = Query.And(sortQuery, queries[0]);
                }
                else
                {
                    var combinedPlayerQuery = Query.And(queries.ToArray());
                    endQuery = Query.And(sortQuery, combinedPlayerQuery);
                }
            }
            else
            {
                endQuery = sortQuery;
            }

            // Query the database
            using (var db = new LiteDatabase(_filePath))
            {
                var fileResults = db.GetCollection<FileResult>("fileResults");

                return fileResults.IncludeAll().Find(endQuery, limit: maxEntries, skip: skip).ToList();
            }
        }

        public void UpdateAlternativeName(string id, string newName)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(id);

            using (var db = new LiteDatabase(_filePath))
            {
                var fileResults = db.GetCollection<FileResult>("fileResults");

                var result = fileResults
                    .IncludeAll()
                    .FindById(id);

                if (result == null)
                {
                    throw new KeyNotFoundException($"Could not find FileResult by id {id}");
                }
                else
                {
                    _log.Information($"Db updating {result.AlternativeName} to {newName}");

                    // Update the file results (for indexing/search)
                    result.AlternativeName = newName;
                    fileResults.Update(result);

                    // Update the replay entry
                    var replays = db.GetCollection<ReplayFile>("replayFiles");
                    var replayEntry = replays
                        .IncludeAll()
                        .FindById(id);
                    replayEntry.AlternativeName = newName;
                    replays.Update(replayEntry);
                }
            }
        }
    }
}
