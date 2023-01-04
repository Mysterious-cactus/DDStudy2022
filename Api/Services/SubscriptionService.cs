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
        public async Task Subscribe(Guid who, SubscriptionModel model)
        {
            var subcription = new Subscription { Who = who, OnWhom = model.OnWhom, Created = model.Created };
            await _context.Subscriptions.AddAsync(subcription);
            //var currentUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == who);
            //var userOnWhom = await _context.Users.FirstOrDefaultAsync(x => x.Id == model.OnWhom);
            //if (currentUser != null && userOnWhom != null)
            //{
            //    currentUser.Subscribes.ToList().Add(model.OnWhom);
            //    userOnWhom.Subscribers.ToList().Add(who);
            //}
            await _context.SaveChangesAsync();
        }
    }
}
