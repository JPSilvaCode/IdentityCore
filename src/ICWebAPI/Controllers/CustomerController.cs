using ICWebAPI.Authorization;
using ICWebAPI.Data;
using ICWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ICWebAPI.Controllers
{
    [Route("[controller]")]
    public class CustomerController : MainController
    {
        private readonly ICMemoryContext _context;

        public CustomerController(ICMemoryContext context)
        {
            _context = context;
        }

        [HttpGet]
        [CustomAuthorize("Customer", "R")]
        public async Task<IEnumerable<Customer>> Get()
        {
            return await _context.Customers.ToListAsync();
        }

        [HttpGet("{id:guid}")]
        [CustomAuthorize("Customer", "R")]
        public async Task<Customer> Get(Guid id)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Id.Equals(id));
        }

        [HttpGet("{email}")]
        [CustomAuthorize("Customer", "R")]
        public async Task<Customer> Get(string email)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Email.Equals(email));
        }

        [HttpPost]
        [CustomAuthorize("Customer", "W")]
        public async Task<IActionResult> Post([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        [CustomAuthorize("Customer", "W")]
        public async Task<IActionResult> Put([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        [Authorize(Policy = "DeleteCustomerPolicy")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id.Equals(id));

            if (customer == null) return NotFound();

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
