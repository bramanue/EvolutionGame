using System.Collections.Generic;

public struct Score
{

	public readonly string Name;

	public readonly long Value;

	public readonly int Rank;

    public Score(string name, long value)
        : this(name, value, -1)
    { }

    public Score(string name, long value, int rank)
    {
        this.Name = name;
		this.Value = value;
        this.Rank = rank;
    }
	
	public override string ToString ()
	{
		return "Score: " + this.Value + ", Name: " + this.Name + ", Rank: " + this.Rank;
	}
}

public class Scores : List<Score>
{ }