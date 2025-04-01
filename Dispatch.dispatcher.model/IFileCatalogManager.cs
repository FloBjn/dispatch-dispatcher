namespace Dispatch.dispatcher.model;

public interface IFileCatalogManager
{
    public List<StoredFile> LoadCatalog(string connectedUser);
}