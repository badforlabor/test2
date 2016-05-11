/****************************************************************************
 * author : auto-gen-tool
 * purpose : 自动生成的代码，不要擅自修改
 * purpose : client_resourcetype
****************************************************************************/
namespace SHGame
{
	// client_resourcetype
	public class SHResourceTypeTemplate : MLICommonObject
	{
		// 注释拉
		public int BigType;
		// ${member.Comment}
		public int SmallType;
		// ${member.Comment}
		public String Icon;
		// ${member.Comment}
		public bool Wan;
		// ${member.Comment}
		public String Name;
		// ${member.Comment}
		public String Desc;
		// ${member.Comment}
		public int ItemId;
		// ${member.Comment}
		public string TipBuy;
		// ${member.Comment}
		public string TipGot;

		public SHResourceTypeTemplate() {}

		public SHResourceTypeTemplate(
				int _BigType,
				int _SmallType,
				String _Icon,
				bool _Wan,
				String _Name,
				String _Desc,
				int _ItemId,
				string _TipBuy,
				string _TipGot
			)
		{		
			BigType = _BigType;
			SmallType = _SmallType;
			Icon = _Icon;
			Wan = _Wan;
			Name = _Name;
			Desc = _Desc;
			ItemId = _ItemId;
			TipBuy = _TipBuy;
			TipGot = _TipGot;
		}

		public void Serialize(MLISerialize ar)
		{				
			CsvSerializeTools.Serialize(ar, ref BigType);
			CsvSerializeTools.Serialize(ar, ref SmallType);
			CsvSerializeTools.Serialize(ar, ref Icon);
			CsvSerializeTools.Serialize(ar, ref Wan);
			CsvSerializeTools.Serialize(ar, ref Name);
			CsvSerializeTools.Serialize(ar, ref Desc);
			CsvSerializeTools.Serialize(ar, ref ItemId);
			CsvSerializeTools.Serialize(ar, ref TipBuy);
			CsvSerializeTools.Serialize(ar, ref TipGot);
		}
	}
}