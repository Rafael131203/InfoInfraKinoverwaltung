using KinoAppCore.Entities;
namespace KinoAppCore.Abstractions;
public interface IKundeRepository: IRepository<Kunde>
{
    Task<Kunde?> FindByEmailAsync(string email);

}
