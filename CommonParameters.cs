using Cocona;

namespace Kurl;

public record CommonParameters([Option('v', Description = "verbose mode")] bool Verbose) : ICommandParameterSet;
