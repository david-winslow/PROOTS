using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OTS.DAL;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OTS.Helpers;
using OTS.ViewModels;

namespace OTS.Controllers
{
  [Route("api/[controller]")]
  public class CustomerController : Controller
  {
    readonly IEmailer _emailer;
    readonly ILogger _logger;
    private IUnitOfWork _unitOfWork;


    public CustomerController(IUnitOfWork unitOfWork, ILogger<CustomerController> logger, IEmailer emailer)
    {
      _unitOfWork = unitOfWork;
      _logger = logger;
      _emailer = emailer;
    }


  


    


    [HttpGet("email")]
    public async Task<string> Email()
    {
      var recepientName = "QickApp Tester"; //         <===== Put the recepient's name here
      var recepientEmail = "test@ebenmonney.com"; //   <===== Put the recepient's email here

      var message = EmailTemplates.GetTestEmail(recepientName, DateTime.UtcNow);

      var response = await _emailer.SendEmailAsync(recepientName, recepientEmail, "Test Email from OTS", message);

      if (response.success)
        return "Success";

      return "Error: " + response.errorMsg;
    }


    // GET api/values/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
      return "value: " + id;
    }


    // POST api/values
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }


    // PUT api/values/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }


    // DELETE api/values/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
