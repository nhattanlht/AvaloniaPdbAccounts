
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;
using ReactiveUI;

namespace AvaloniaPdbAccounts.ViewModels.PCTSV
{
    public partial class StudentsPCTSVViewModel : ViewModelBase
    {
        public ObservableCollection<StudentModel> Students { get; }
        public ReactiveCommand<StudentModel, Unit> EditCommand { get; }
        public ReactiveCommand<StudentModel, Unit> DeleteCommand { get; }

        private readonly UserService _userService;

        public StudentsPCTSVViewModel()
        {
            _userService = new UserService();

            Students = new ObservableCollection<StudentModel>();

            EditCommand = ReactiveCommand.Create<StudentModel>(student =>
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    await _userService.UpdateStudentAsync(
                        student.ID,
                        student.ADDRESS,
                        student.PHONE,
                        student.STATUS
                    );

                    Console.WriteLine($"Updated student: {student.ID} - {student.NAME}");
                });
            });
            DeleteCommand = ReactiveCommand.Create<StudentModel>(student =>
            {

                _ = Task.Run(async () =>
                {
                    try
                    {
                        bool success = await _userService.DeleteStudentAsync(student.ID);
                        if (success)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Students.Remove(student);
                                Console.WriteLine(" Deleted student: " + student.ID + " - " + student.NAME);
                            });
                        }
                        else
                        {
                            Console.WriteLine("Delete failed for student: " + student.ID + " - " + student.NAME);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("❗ Lỗi khi xóa sinh viên: " + student.ID);
                        
                    }
                });
            });

                // Load when ViewModel is created
                _ = LoadStudentsAsync();
        }

        public async Task LoadStudentsAsync()
        {
            try
            {
                var students = await _userService.GetStudentModelDataAsync();


                Dispatcher.UIThread.Post(() =>
                {
                    Students.Clear();
                    foreach (var student in students)
                    {
                        Students.Add(student);
                    }
                });
            }
            catch (Exception ex)
            {
                // In lỗi ra console hoặc log
                Console.WriteLine("Lỗi khi tải danh sách sinh viên: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);

               
            }
        }
    }
}
