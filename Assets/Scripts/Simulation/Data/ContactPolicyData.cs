public class ContactPolicyData : DataModule
{
    public bool visibleToAll = true;
	public bool openlyHostile = false;

	public override string Read()
	{
		return $"	Visible To All: {visibleToAll}\n" +
			$"	Openly Hostile: {openlyHostile}";
	}
}