namespace _Project.Scripts.Interfaces
{
    public interface ISavableLogic
    {
        public ISavableModel GetSavableModel();
        public void SetSavableModel(ISavableModel savableModel);
    }
}