﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CentCom.Common.Configuration
{
    public enum DbType
    {
        Postgres
    }

    public class DbConfig
    {
        public string ConnectionString { get; set; }
        public DbType DbType { get; set; }
    }
}
