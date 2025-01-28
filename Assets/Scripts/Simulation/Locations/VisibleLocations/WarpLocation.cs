using EntityAndDataDescriptor;

public class WarpLocation : VisitableLocation
{
	public override string GetTitle()
	{
		return "The Warp";
	}

	public override string GetDescription()
	{
		return "Quiet.";
	}

	public override string GetExtraInformation()
	{
		return "The Warp is used to facillitate travel over long distances. While inside The Warp, observers outside of it appear accelerated, moving 10 times faster than normal.";
	}

	public override Shop GetShop()
	{
		Warp.main.GetData(DataTags.CentralShop, out Shop shop);
		return shop;
	}
}
