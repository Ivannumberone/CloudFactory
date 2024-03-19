using CloudFactory.Services;

using Microsoft.AspNetCore.Mvc;

namespace CloudFactory.Controllers
{
	[Route("[controller]/[action]")]
	[ApiController]
	public class MessageController : ControllerBase
	{
		private BrokerService _brokerService;

		public MessageController(BrokerService brokerService)
		{
			_brokerService = brokerService;
		}

		[HttpGet]
		public ActionResult<string> GetResponse(string key)
		{
			return _brokerService.GetResponse(key);
		}

		[HttpPost]
		public ActionResult<string> PostRequestAdvanced()
		{
			return _brokerService.CreateRequest(Request);
		}

		[HttpPost]
		public ActionResult<string> Test1()
		{
			return _brokerService.CreateRequest(Request);
		}
		[HttpPost]
		public ActionResult<string> Test2()
		{
			return _brokerService.CreateRequest(Request);
		}
	}
}
