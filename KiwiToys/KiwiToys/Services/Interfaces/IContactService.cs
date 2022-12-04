using KiwiToys.Models;

namespace KiwiToys.Services {
    public interface IContactService {
        string SendMessage(ContactViewModel contactViewModel);
    }
}