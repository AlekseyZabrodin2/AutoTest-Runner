using AutoTestRunner.Views;
using AventStack.ExtentReports.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace AutoTestRunner.Model.TestConfig
{
    public partial class TestsConstData : ObservableObject
    {

        public TestRepeatTestSetNumberOfTimes TestRepeatTestSetNumberOfTimes { get; set; } = new();

        public TestEngLowCaseData TestEngLowCaseData { get; set; } = new();

        public TestEngUpperCaseData TestEngUpperCaseData { get; set; } = new();

        public TestRusLowCaseData TestRusLowCaseData { get; set; } = new();

        public TestRusUpperCaseData TestRusUpperCaseData { get; set; } = new();

        public TestNameLongerThan64SymbolData TestNameLongerThan64SymbolData { get; set; } = new();

        public TestNewPatientWithRandomData TestNewPatientWithRandomData { get; set; } = new();
        



        public string? DateOfBirth { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Comments { get; set; }

        public string? PatientSearch { get; set; }

        public string? WorklistSearch { get; set; }

        public string? StartDateText { get; set; }

        public string? EndDateText { get; set; }

        public string? StudiesStartDateText { get; set; }

        public string? StudiesEndDateText { get; set; }

        public string? StudyInfoStudyId { get; set; }

        public string? StudyInfoAccessionNumber { get; set; }

        public string? StudyInfoDescription { get; set; }

        public string? StudyInfoProcedureDescriptionRus { get; set; }

        public string? StudyInfoProcedureDescriptionEng { get; set; }

        public string? StudyInfoPerformingPhysician { get; set; }

        public string? StudyInfoPerformingPhysicianInvalid { get; set; }

        public string? StudyInfoRefferingPhysician { get; set; }

        public string? StudyInfoRefferingPhysicianInvalid { get; set; }

        public string? TemplateNameRus { get; set; }

        public string? TemplateNameEng { get; set; }

        public string? TemplateNameSpace { get; set; }

        public string? TemplateNameSpecialCharacters { get; set; }

        public string? TemplateNameWithNumbers { get; set; }

        public string? TemplateNameNull { get; set; }

        public string? AuthenticateStateUserName { get; set; }

        public string? AuthenticateStateUserPassword { get; set; }

        public string? NewUsersOperatorLogin { get; set; }

        public string? NewUsersAdminLogin { get; set; }

        public string? NewUsersPassword { get; set; }

        public string? NewUsersFirstName { get; set; }

        public string? NewUsersMiddleName { get; set; }

        public string? NewUsersLastName { get; set; }

        public string? PasswordChange { get; set; }





    }
}
