using CentCom.Common.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentCom.Server.FlatData
{
    /// <summary>
    /// Used for importing flat-format ban data provided from old, inactive, or difficult to parse servers
    /// </summary>
    public class FlatDataImporter
    {
        private ILogger _logger;
        private DatabaseContext _dbContext;

        public FlatDataImporter(DatabaseContext dbContext, ILogger<FlatDataImporter> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task RunImports()
        {
            var toImport = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => typeof(IFlatDataSource).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToList();

            foreach (var fds in toImport)
            {
                string sourceName = null;
                try
                {
                    var dataSource = (IFlatDataSource)Activator.CreateInstance(fds);
                    sourceName = dataSource.SourceDisplayName();
                    var source = dataSource.GetSources();

                    // Check if we've already imported this dataset
                    var sourceNames = source.Select(x => x.Name);
                    if (_dbContext.BanSources.Any(x => sourceNames.Contains(x.Name)))
                    {
                        continue;
                    }

                    _logger.LogInformation($"Importing flat ban data for {sourceName}");

                    // Import new sources
                    _dbContext.BanSources.AddRange(source);
                    await _dbContext.SaveChangesAsync();

                    // Using new source IDs, import bans
                    var newBans = dataSource.GetBans();
                    foreach (var b in newBans)
                    {
                        b.SourceNavigation = source.First(x => x.Name == b.SourceNavigation.Name);
                        b.Source = b.SourceNavigation.Id;
                    }
                    _dbContext.Bans.AddRange(newBans);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Import complete");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to import flat ban data from {sourceName ?? "[no name found for importer]"}, see below for details");
                }
            }
        }
    }
}
