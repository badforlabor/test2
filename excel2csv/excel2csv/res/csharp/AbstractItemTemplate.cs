/****************************************************************************
 * author : auto-gen-tool
 * purpose : 自动生成的代码，不要擅自修改
 * purpose : client_AbstractItemTemplate
****************************************************************************/
namespace SHGame
{
	// client_AbstractItemTemplate
	public class AbstractItemTemplate : MLICommonObject
	{
		// 注释拉
		public int FeatureResource;
		// ${member.Comment}
		public int ClientMsg;
		// ${member.Comment}
		public String Desc;

		public AbstractItemTemplate() {}

		public AbstractItemTemplate(
				int _FeatureResource,
				int _ClientMsg,
				String _Desc
			)
		{		
			FeatureResource = _FeatureResource;
			ClientMsg = _ClientMsg;
			Desc = _Desc;
		}

		public void Serialize(MLISerialize ar)
		{				
			CsvSerializeTools.Serialize(ar, ref FeatureResource);
			CsvSerializeTools.Serialize(ar, ref ClientMsg);
			CsvSerializeTools.Serialize(ar, ref Desc);
		}
	}
}