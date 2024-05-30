using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Godot;
using FileAccess = Godot.FileAccess;

public partial class FileProcessor : Node
{
    private readonly string filename;
    private ulong lastPosition;

    public FileProcessor(string filename)
    {
        this.filename = filename;
        lastPosition = 0;
    }

    public async Task<List<Segment>> GetSegmentsAsync()
    {
        var segments = new List<Segment>();

        try
        {
            // If it's the first time reading, read the entire file
            if (lastPosition == 0)
            {
                await ReadFromFileAsync(segments, 0, true);
            }
            else
            {
                // Read only new segments since last position
                await ReadFromFileAsync(segments, lastPosition, false);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error reading file: {e.Message}");
        }

        return segments;
    }

    private async Task ReadFromFileAsync(List<Segment> segments, ulong startPosition, bool skipHeader)
    {
        using (var file = FileAccess.Open(filename, FileAccess.ModeFlags.Read))
        {
            file.Seek(startPosition);

            using (var reader = new StreamReader(new MemoryStream(file.GetBuffer((long)file.GetLength()))))
            {
                string line;
                if (skipHeader)
                {
                    // Read and validate the header lines
                    var expectedHeaders = new List<string>
                    {
                        "version 2",
                        "kind,name,owner,faction,factionColor,entityId,health,position,rotation"
                    };

                    for (int i = 0; i < expectedHeaders.Count; i++)
                    {
                        line = await reader.ReadLineAsync();
                        if (line != expectedHeaders[i])
                        {
                            GD.PrintErr($"Error: expected '{expectedHeaders[i]}', got '{line}'");
                        }
                    }
                }

                Segment segment = null;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.StartsWith("start_block"))
                    {
                        if (segment != null)
                        {
                            segments.Add(segment);
                        }
                        segment = new Segment();
                    }
                    segment?.Entries.Add(ProcessLine(line));
                }
                if (segment != null)
                {
                    segments.Add(segment);
                }
                lastPosition = file.GetPosition();
            }
        }
    }

    private string ProcessLine(string line)
    {
        // Implement your line processing logic here
        return line;
    }
}

public partial class Segment
{
    public List<string> Entries { get; } = new List<string>();

    // Additional properties and methods for segment processing can be added here
}
