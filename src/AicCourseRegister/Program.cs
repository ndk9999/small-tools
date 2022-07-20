using System.Globalization;
using AicCourseRegister;
using CsvHelper;
using Microsoft.Playwright;

if (args.Length == 0)
{
	Console.WriteLine("Please provide input CSV file");
	return;
}

var csvFilePath = args[0];
if (!File.Exists(csvFilePath))
{
	Console.WriteLine("Invalid CSV file path");
	return;
}

using var reader = new StreamReader(csvFilePath);
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
var students = csv.GetRecords<Student>();

if (!students.Any())
{
	Console.WriteLine("No students found");
	return;
}

if (args.Length > 1 && int.TryParse(args[1], out var skip))
{
	students = students.Skip(skip).ToList();
}

var userDataDir = Path.Combine(AppContext.BaseDirectory, ".user-data", "profile");
var downloadDir = Path.Combine(AppContext.BaseDirectory, ".user-data", "downloads");
var options = new BrowserTypeLaunchPersistentContextOptions
{
	Headless = false,
	Channel = "chrome",
	AcceptDownloads = true,
	DownloadsPath = downloadDir
};

var playwright = await Playwright.CreateAsync();
var browser = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, options);

browser.SetDefaultTimeout(90000);
browser.SetDefaultNavigationTimeout(90000);

var page = await browser.NewPageAsync();

foreach (var student in students)
{
	await page.GotoAsync("http://aic.dlu.edu.vn/vi/register-courses.html");

	await page.FillAsync("#register-courses-email-input", student.Email);
	await page.FillAsync("#register-courses-phone-input", student.Phone);
	await page.FillAsync("#register-courses-full-name-input-hv-0", student.FullName);

	if (student.Gender != "Nam")
	{
		await page.ClickAsync("#female-gender-hv-0");
	}

	var courseIdx = Random.Shared.Next(4);
	await page.ClickAsync($"#course-checkbox-{courseIdx}-hv-0");

	var dob = DateTime.ParseExact(student.Dob, "dd/MM/yyyy", CultureInfo.CurrentCulture);
	await page.EvaluateAsync("([ns]) => document.getElementById('register-courses-date-input-hv-0').value = ns;", new[] {dob.ToString("yyyy-MM-dd")});

	await page.ClickAsync("#submit-register-courses");
	await Task.Delay(2000);
}