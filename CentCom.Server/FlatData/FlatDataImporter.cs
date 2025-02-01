using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CentCom.Common.Data;
using CentCom.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CentCom.Server.FlatData;

/// <summary>
/// Used for importing flat-format ban data provided from old, inactive, or difficult to parse servers
/// </summary>
public class FlatDataImporter
{
    private readonly DatabaseContext _dbContext;
    private readonly ILogger _logger;

    public FlatDataImporter(DatabaseContext dbContext, ILogger<FlatDataImporter> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task RunImports()
    {
        foreach (var file in Directory.GetFiles("FlatData/JSON/"))
        {
            var fileData = await File.ReadAllTextAsync(file);
            FlatDataFile deserializedData;
            try
            {
                deserializedData = JsonSerializer.Deserialize<FlatDataFile>(fileData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            }
            catch (Exception)
            {
                _logger.LogError("Failed to deserialize flat data file: '{File}'", file);
                continue;
            }

            var lastVersion = await _dbContext.FlatBansVersion
                .Where(x => x.Name == deserializedData.Name)
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync();

            // We need to update the data
            if (lastVersion == null || lastVersion.Version < deserializedData.Version)
            {
                if (lastVersion == null)
                    _logger.LogInformation("Data from '{Name}' missing from database, adding...", deserializedData.Name);
                else
                    _logger.LogInformation("Out-of-date data found from '{Name} (v{LastVersion}, updating to v{NewVersion}), updating...'", deserializedData.Name, lastVersion.Version, deserializedData.Version);

                try
                {
                    // Make this an atomic operation to ensure failed imports don't leave holes in the data
                    await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                    // Update any ban sources if necessary
                    var toUpdate = await _dbContext.BanSources
                        .Where(x => deserializedData.Sources.Select(y => y.Name).Contains(x.Name))
                        .Include(x => x.Bans)
                        .ToListAsync();
                    foreach (var source in toUpdate)
                    {
                        var match = deserializedData.Sources.First(x => x.Name == source.Name);
                        if (match.RoleplayLevel != source.RoleplayLevel || match.Display != source.Display)
                        {
                            _logger.LogInformation("Updating ban source '{Name}', found mis-matching metadata with new version of flat dataset", source.Name);
                            source.Display = match.Display;
                            source.RoleplayLevel = match.RoleplayLevel;
                        }
                    }
                    await _dbContext.SaveChangesAsync();

                    // Remove any existing bans for each updated source
                    foreach (var source in toUpdate)
                    {
                        _dbContext.Bans.RemoveRange(source.Bans);
                    }
                    await _dbContext.SaveChangesAsync();

                    // Add any new ban sources that were not present in old file version
                    foreach (var source in deserializedData.Sources.Where(x => !toUpdate.Select(y => y.Name).Contains(x.Name)))
                    {
                        var copy = new BanSource
                        {
                            Display = source.Display,
                            Name = source.Name,
                            RoleplayLevel = source.RoleplayLevel
                        };
                        _dbContext.BanSources.Add(copy);
                        toUpdate.Add(copy);
                    }
                    await _dbContext.SaveChangesAsync();

                    // Map bans to ban sources
                    foreach (var group in deserializedData.ServerBans.Concat(deserializedData.JobBans).GroupBy(x => x.Source))
                    {
                        var matchingSource = deserializedData.Sources.First(x => x.Id == group.Key);
                        var matchingDbSource = toUpdate.First(x => x.Name == matchingSource.Name);

                        foreach (var b in group)
                        {
                            b.Source = matchingDbSource.Id;
                            b.SourceNavigation = matchingDbSource;
                        }

                        _dbContext.Bans.AddRange(group);
                    }
                    await _dbContext.SaveChangesAsync();

                    // Add update record reflecting the changes
                    var thisUpdate = new FlatBansVersion
                    {
                        Name = deserializedData.Name,
                        Version = deserializedData.Version,
                        PerformedAt = DateTime.UtcNow
                    };
                    _dbContext.FlatBansVersion.Add(thisUpdate);
                    await _dbContext.SaveChangesAsync();

                    // Commit changes when everything else has gone correctly
                    await transaction.CommitAsync();

                    _logger.LogInformation("Import/update complete");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to import flat ban data from {deserializedData.Name}, see below for details");
                }
            }
        }
    }
}