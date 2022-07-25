using AspNet_Redis.Context.Models;
using AspNet_Redis.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace AspNet_Redis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private readonly DataContext _context;
        public OrderController(DataContext context, IDistributedCache distributedCache)
        {
            _context = context;
            _distributedCache = distributedCache;
        }

        [HttpGet("redis")]
        public async Task<IActionResult> GetAllOrdersUsingRedisCache()
        {
            try
            {
                var cacheKey = "orderList";
                string serializedOrderList;
                var orderList = new List<Order>();

                var redisOrderList = await _distributedCache.GetAsync(cacheKey);

                if (redisOrderList != null)
                {
                    serializedOrderList = Encoding.UTF8.GetString(redisOrderList);
                    orderList = JsonConvert.DeserializeObject<List<Order>>(serializedOrderList);
                }
                else
                {
                    orderList = await _context.Orders.ToListAsync();
                    serializedOrderList = JsonConvert.SerializeObject(orderList);
                    redisOrderList = Encoding.UTF8.GetBytes(serializedOrderList);

                    var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                                        .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                    await _distributedCache.SetAsync(cacheKey, redisOrderList, options);
                }

                return Ok(orderList);
            }
            catch (Exception)
            {
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError), "Erro ao tentar obter pedidos do banco de dados.");
            }

        }

        //[HttpGet("orders")]
        //public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        //{
        //    try
        //    {
        //        var orders = await _context.Orders.AsNoTracking().ToListAsync();
        //        return Ok(orders);
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError), "Erro ao tentar obter pedidos do banco de dados.");
        //    }
        //}
    }
}
