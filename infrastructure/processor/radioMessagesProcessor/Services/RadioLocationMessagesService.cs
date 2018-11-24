using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RadioMessagesProcessor.Entities;
using RadioMessagesProcessor.Helpers;

namespace RadioMessagesProcessor.Services
{
    public interface IRadioLocationMessagesService
    {
        Task<int> InsertAsync(RadioLocationMessage radioLocationMessage);
        List<RadioLocationMessage> GetByImeiAndDateTimeRange(string imei, DateTime fromUtc, DateTime toUtc);

        RadioLocationMessage GetById(Guid id);
        void Update(RadioLocationMessage locationMessage);
        void Delete(Guid id);
    }

    public class RadioLocationMessagesService : IRadioLocationMessagesService
    {
        private DataContext _context;

        public RadioLocationMessagesService(DataContext context)
        {
            _context = context;
        }

        public async Task<int> InsertAsync(RadioLocationMessage radioLocationMessage)
        {
            await _context.RadioLocationMessages.AddAsync(radioLocationMessage).ConfigureAwait(false);
            return await _context.SaveChangesAsync().ConfigureAwait(false);

        }

        public void Delete(Guid id)
        {
            var radioLocationMessage = _context.RadioLocationMessages.Find(id);
            if (radioLocationMessage != null)
            {
                _context.RadioLocationMessages.Remove(radioLocationMessage);
                _context.SaveChanges();
            }
        }

        public RadioLocationMessage GetById(Guid id)
        {
            return _context.RadioLocationMessages.Find(id);
        }

        public void Update(RadioLocationMessage locationMessage)
        {
            throw new NotImplementedException();
        }

        public List<RadioLocationMessage> GetByImeiAndDateTimeRange(string imei, DateTime fromUtc, DateTime toUtc)
        {
            throw new NotImplementedException();
        }
    }
}