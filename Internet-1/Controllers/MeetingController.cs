// MeetingController.cs
using AutoMapper;
using Internet_1.Models;
using Internet_1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Internet_1.Repositories;
using Microsoft.AspNetCore.Authorization;


namespace Internet_1.Controllers
{
    [Authorize]
    public class MeetingController : Controller
    {
        private readonly MeetingRepository _meetingRepository;
        private readonly IMapper _mapper;
        ResultModel resultModel = new ResultModel();

        public MeetingController(MeetingRepository meetingRepository, IMapper mapper)
        {
            _meetingRepository = meetingRepository;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ListAjax()
        {
            var currentUser = User.Identity.Name; // Oturum açan kullanıcı
            var meetings = await _meetingRepository.GetAllAsync();
            var userMeetings = meetings.Where(m => m.CreatedBy == currentUser).ToList(); // Filtreleme
            var meetingModels = _mapper.Map<List<MeetingModel>>(userMeetings);
            return Json(meetingModels);
        }

        public async Task<IActionResult> GetByIdAjax(int id)
        {
            var currentUser = User.Identity.Name;
            var meeting = await _meetingRepository.GetByIdAsync(id);

            if (meeting == null || meeting.CreatedBy != currentUser) // Kullanıcı doğrulaması
            {
                return Json(new { Status = false, Message = "Yetkiniz yok!" });
            }

            var meetingModel = _mapper.Map<MeetingModel>(meeting);
            return Json(meetingModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateAjax(MeetingModel model)
        {
            var currentUser = User.Identity.Name; // Oturum açan kullanıcıyı al

            if (model.Id == 0)
            {
                var meeting = new Meeting
                {
                    Title = model.Title,
                    Description = model.Description,
                    MeetingDate = DateTime.Parse(model.MeetingDate),
                    MeetingTime = TimeSpan.Parse(model.MeetingTime),
                    IsActive = true,
                    Created = DateTime.Now,
                    Updated = DateTime.Now,
                    CreatedBy = currentUser // Kullanıcıyı kaydet
                };

                await _meetingRepository.AddAsync(meeting);
                resultModel.Status = true;
                resultModel.Message = "Toplantı Eklendi";
            }
            else
            {
                var meeting = await _meetingRepository.GetByIdAsync(model.Id);
                if (meeting == null)
                {
                    resultModel.Status = false;
                    resultModel.Message = "Kayıt Bulunamadı!";
                    return Json(resultModel);
                }

                meeting.Title = model.Title;
                meeting.Description = model.Description;
                meeting.MeetingDate = DateTime.Parse(model.MeetingDate);
                meeting.MeetingTime = TimeSpan.Parse(model.MeetingTime);
                meeting.Updated = DateTime.Now;

                await _meetingRepository.UpdateAsync(meeting);
                resultModel.Status = true;
                resultModel.Message = "Toplantı Düzenlendi";
            }

            return Json(resultModel);
        }

        public async Task<IActionResult> DeleteAjax(int id)
        {
            var currentUser = User.Identity.Name; // Oturum açan kullanıcı
            var meeting = await _meetingRepository.GetByIdAsync(id);

            if (meeting == null || meeting.CreatedBy != currentUser) // Kullanıcı doğrulaması
            {
                resultModel.Status = false;
                resultModel.Message = "Yetkiniz yok veya kayıt bulunamadı!";
                return Json(resultModel);
            }

            await _meetingRepository.DeleteAsync(id);
            resultModel.Status = true;
            resultModel.Message = "Toplantı Silindi";
            return Json(resultModel);
        }
    }
}