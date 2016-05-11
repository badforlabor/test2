/****************************************************************************
 * author : auto-gen-tool
 * purpose : 自动生成的代码，不要擅自修改
 * purpose : client_prestige_config
****************************************************************************/
namespace SHGame
{
	// client_prestige_config
	public class SHPrestigeTemplate : MLICommonObject
	{
		// 声望分段名称
		public string Name;
		// ${member.Comment}
		public int MinValue;
		// ${member.Comment}
		public int MaxValue;
		// ${member.Comment}
		public string Color;

		public SHPrestigeTemplate() {}

		public SHPrestigeTemplate(
				string _Name,
				int _MinValue,
				int _MaxValue,
				string _Color
			)
		{		
			Name = _Name;
			MinValue = _MinValue;
			MaxValue = _MaxValue;
			Color = _Color;
		}

		public void Serialize(MLISerialize ar)
		{				
			CsvSerializeTools.Serialize(ar, ref Name);
			CsvSerializeTools.Serialize(ar, ref MinValue);
			CsvSerializeTools.Serialize(ar, ref MaxValue);
			CsvSerializeTools.Serialize(ar, ref Color);
		}
	}
}