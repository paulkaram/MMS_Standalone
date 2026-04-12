namespace Intalio.Tools.Common.FileKit.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class PrimaryKeyForSheet : Attribute
	{
		public string Name { get; set; }

		public PrimaryKeyForSheet(string name)
		{
			Name = name;
		}
	}
}
