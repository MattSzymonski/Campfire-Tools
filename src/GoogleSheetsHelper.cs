using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using GetRequest = Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.GetRequest;
using UpdateRequest = Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest;

namespace CampfireTools.GoogleSheets
{
    public class GoogleSheetsHelper 
    {
        public static SheetsService Login(string programName, string credentialsFilePath) 
        {
            string jsonContent = File.ReadAllText(credentialsFilePath);
            ServiceAccountCredential serviceAccountCredential = (ServiceAccountCredential)GoogleCredential.FromJson(jsonContent).UnderlyingCredential;

            return new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = serviceAccountCredential,
                ApplicationName = programName
            });
        }

        public static List<string> ReadWorksheetColumnData(SheetsService sheetsService, string spreadsheetId, string worksheetName, string columnLetter)
        {
            var range = $"{columnLetter}:{columnLetter}";
            return ReadWorksheetData(sheetsService, spreadsheetId, worksheetName, range, GetRequest.MajorDimensionEnum.COLUMNS);
            //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response.Values));
        }

        public static List<string> ReadWorksheetRowData(SheetsService sheetsService, string spreadsheetId, string worksheetName, int rowNumber)
        {
            var range = $"{rowNumber}:{rowNumber}";
            return ReadWorksheetData(sheetsService, spreadsheetId, worksheetName, range, GetRequest.MajorDimensionEnum.ROWS);
        }

        private static List<string> ReadWorksheetData(SheetsService sheetsService, string spreadsheetId, string worksheetName, string range, GetRequest.MajorDimensionEnum dimension)
        {
            GetRequest? request = sheetsService.Spreadsheets.Values.Get(spreadsheetId, $"{worksheetName}!{range}");
            request.MajorDimension = dimension;
            request.ValueRenderOption = GetRequest.ValueRenderOptionEnum.FORMATTEDVALUE;
            request.DateTimeRenderOption = GetRequest.DateTimeRenderOptionEnum.FORMATTEDSTRING;
            ValueRange response = request.Execute();
            List<string> data = response.Values[0].ToList().ConvertAll(obj => (string)obj);
            return data;
        }

        public static bool WriteWorksheetRow(SheetsService sheetsService, string spreadsheetId, string worksheetName, int rowNumber, List<string> data) 
        {
            var range = $"{rowNumber}:{rowNumber}";
            ValueRange valueRange = new ValueRange { Values = new List<IList<object>> { data.ConvertAll(s => (object)s) } };
            UpdateRequest? request = sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetId, $"{worksheetName}!{range}");
            request.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
            UpdateValuesResponse response = request.Execute();
            return response.UpdatedCells > 0;
        }

        public static int GetFirstFreeRowNumberInColumn(SheetsService sheetsService, string spreadsheetId, string worksheetName, string columnLetter) 
        {
            List<string> data = ReadWorksheetColumnData(sheetsService, spreadsheetId, worksheetName, columnLetter);
            int index = data.FindIndex(v => v == "");
            return index == -1 ? data.Count + 1 : index + 1;
        }

        public static bool DeleteWorksheetRows(SheetsService sheetsService, string spreadsheetId, string worksheetName, List<int> rowNumbers) 
        {
            var deleteRequest = new BatchUpdateSpreadsheetRequest();
            deleteRequest.Requests = new List<Request>();

            foreach (int rowNumber in rowNumbers)
            {
                var deleteDimensionRequest = new DeleteDimensionRequest
                {
                    Range = new DimensionRange
                    {
                        SheetId = GetSheetId(sheetsService, spreadsheetId, worksheetName),
                        Dimension = "ROWS",
                        StartIndex = rowNumber - 1,
                        EndIndex = rowNumber
                    }
                };

                deleteRequest.Requests.Add(new Request
                {
                    DeleteDimension = deleteDimensionRequest
                });
            }

            var batchUpdateRequest = sheetsService.Spreadsheets.BatchUpdate(deleteRequest, spreadsheetId);
            var batchUpdateResponse = batchUpdateRequest.Execute();

            return batchUpdateResponse.Replies.Count > 0;
        }

        private static int GetSheetId(SheetsService sheetsService, string spreadsheetId, string worksheetName)
        {
            var spreadsheet = sheetsService.Spreadsheets.Get(spreadsheetId).Execute();
            var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == worksheetName);
            return sheet?.Properties.SheetId ?? -1;
        }
    }
}