﻿using System;

namespace Rofl.Files.Models
{
    public class FileErrorResult
    {
        /// <summary>
        /// Offending file path
        /// </summary>
        public string FilePath { get; set; }

        public Exception Exception { get; set; }
    }
}
