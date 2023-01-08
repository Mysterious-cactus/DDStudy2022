using Api.Models.Subcribes;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class SubscriptionService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public SubscriptionService(DataContext context, IMapper mapper)
        {

            _context = context;
            _mapper = mapper;
        }

        public async Task UnSubscribe(Guid userId, Guid fromWhom)
        {
            var sub = await _context.Subscriptions.FirstOrDefaultAsync(x => x.OnWhom == fromWhom && x.Who == userId);
            if (sub != null)
            {
                _context.Remove(sub);
                await _context.SaveChangesAsync();
            }
        }
        public async Task Subscribe(Guid who, Guid onWhom)
        {
            var subscription = new Subscription { Who = who, OnWhom = onWhom, Created = DateTimeOffset.UtcNow};
            await _context.Subscriptions.AddAsync(subscription);
            await _context.SaveChangesAsync();
        }
    }
}
