/****************************************************************************
 * author : auto-gen-tool
 * purpose : 自动生成的代码，不要擅自修改
 * purpose : client_constant
****************************************************************************/
namespace SHGame
{
	// client_constant
	public class SHConstantTemplate : MLICommonObject
	{
		// 注释拉
		public string Key;
		// ${member.Comment}
		public string Value;

		public SHConstantTemplate() {}

		public SHConstantTemplate(
				string _Key,
				string _Value
			)
		{		
			Key = _Key;
			Value = _Value;
		}

		public void Serialize(MLISerialize ar)
		{				
			CsvSerializeTools.Serialize(ar, ref Key);
			CsvSerializeTools.Serialize(ar, ref Value);
		}
	}
}