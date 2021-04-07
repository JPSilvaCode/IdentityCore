using ICWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICWebAPI.Controllers
{
    [Route("[controller]")]
    public class CustomerController : MainController
    {
        private readonly IList<Customer> _customers;

        public CustomerController()
        {
            if (_customers != null) return;

            this._customers = new List<Customer>();

            var customer = new Customer
            {
                Id = Guid.Parse("ee282ec4-a342-4755-8fe5-06e25d50a29c"),
                Email = "jpsilvafla@gmail.com",
                Name = "Joao Paulo"
            };
            this._customers.Add(customer);

            var customer2 = new Customer
            {
                Id = Guid.Parse("8f9fda7f-18f7-4fae-a533-7e34a66788a9"),
                Email = "jpsilvafla@hotmail.com",
                Name = "Joao Paulo 2"
            };
            this._customers.Add(customer2);
        }

        [HttpGet]
        [Authorize]
        public IEnumerable<Customer> Get()
        {
            return this._customers;
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public Customer Get(Guid id)
        {
            return this._customers.FirstOrDefault(c => c.Id.Equals(id));
        }

        [HttpGet("{email}")]
        [Authorize]
        public Customer Get(string email)
        {
            return this._customers.FirstOrDefault(c => c.Email.Equals(email));
        }

        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            this._customers.Add(customer);

            return Ok();
        }

        [HttpPut]
        [Authorize]
        public IActionResult Put([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var customerUpdate = this._customers.FirstOrDefault(c => c.Id.Equals(customer.Id));

            if (customerUpdate == null) return NotFound();

            customerUpdate.Name = customer.Name;
            customerUpdate.Email = customer.Email;

            return Ok();
        }

        [HttpDelete]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var customer = this._customers.FirstOrDefault(c => c.Id.Equals(id));

            if (customer == null) return NotFound();

            this._customers.Remove(customer);

            return Ok();
        }
    }
}
