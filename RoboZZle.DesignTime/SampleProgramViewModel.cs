#pragma warning disable SA1402 // File may only contain a single class

namespace RoboZZle.DesignTime;

public class SampleProgramViewModel(): ProgramViewModel(SampleProgram.Instance);

public class SampleEmptyProgramViewModel(): ProgramViewModel(SamplePuzzle.EmptyProgram);