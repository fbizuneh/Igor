
namespace Igor
{
	public interface IIgorModule
	{
		string GetModuleName();

		void RegisterModule();
		void ProcessArgs(IIgorStepHandler StepHandler);

		string DrawJobInspectorAndGetEnabledParams(string CurrentParams);
	}
}