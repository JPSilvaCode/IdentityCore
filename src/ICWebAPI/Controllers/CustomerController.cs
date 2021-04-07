using ICWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICWebAPI.Controllers
{
    [Route("[controller]")]
    public class CustomerController : Controller
    {
        private IList<Customer> customers = null;

        public CustomerController()
        {
            this.customers = new List<Customer>();

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Email = "jpsilvafla@gmail.com",
                Name = "Joao Paulo"
            };
            this.customers.Add(customer);

            var customer2 = new Customer
            {
                Id = Guid.NewGuid(),
                Email = "jpsilvafla@hotmail.com",
                Name = "Joao Paulo 2"
            };
            this.customers.Add(customer2);
        }

        [HttpGet]
        [Authorize]
        public IEnumerable<Customer> Get()
        {
            return this.customers;
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public Customer Get(Guid id)
        {
            return this.customers.FirstOrDefault(c => c.Id.Equals(id));
        }
    }
}
