using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using WebMatrix.WebData;
using System.Net.Mail;
using System.Net;

/// <summary>
/// Summary description for Functions
/// </summary>
public class Functions
{
    //------------------------------------------LOGIN FUNCTION
    public static bool Login(string email, string password)
    {
        // If this is a POST request, validate and process data
        AntiForgery.Validate();
        // is this an external login request?
        string emailtemp = email.Split('@').First();
        if (!CheckIfAvaillable(emailtemp, "Employee", "Email"))
        {
            string query = string.Format("SELECT Password FROM Employee WHERE Email LIKE '{0}'", email);
            string storedpassword = Connection.ExecuteQuery(query).Rows[0].ItemArray[0].ToString();

            if (Connection.Hash(password) == storedpassword)
            {
                return true;
            }
        }
        return false;
    }


    //------------------------------------------REGISTER FUNCTION
    public static void Register(string email, string password, string firstName, string lastName, string department)
    {
        // If this is a POST request, validate and process data
        AntiForgery.Validate();
        // Insert a new user into the database       
        // REGISTER
        password = Connection.Hash(password);
        string query = string.Format("INSERT INTO employee (Email, FirstName, LastName, Password) VALUES ('{0}','{1}','{2}','{3}')", email, firstName, lastName, password);
        Connection.ExecuteNonQuery(query);

        query = string.Format("SELECT Employee_ID FROM employee WHERE LOWER(Email) LIKE LOWER('{0}%')", email);
        DataTable result = Connection.ExecuteQuery(query);
        int e_id = int.Parse(result.Rows[0].ItemArray[0].ToString());

        query = string.Format("SELECT Department_ID FROM Department WHERE LOWER(Name) = LOWER('{0}')", department);
        result = Connection.ExecuteQuery(query);
        int d_id = int.Parse(result.Rows[0].ItemArray[0].ToString());

        query = string.Format("INSERT INTO Employee_has_department(Employee_ID, Department_ID) Values({0}, {1})", e_id, d_id);
        Connection.ExecuteNonQuery(query);

        //if (role == 4)
        //{
        //    query = string.Format("INSERT INTO manager(Employee_ID, Department_ID) Values({0}, {1})", e_id, d_id);
        //    Connection.ExecuteNonQuery(query);
        //}
        //if (role == 5)
        //{
        //    query = string.Format("INSERT INTO financepayer(Employee_ID) Values({0})", e_id);
        //    Connection.ExecuteNonQuery(query);
        //}
    }

    //-------------SHOW AND HIDE SECTIONS
    public static string HideShow(string value)
    {
        if (value == "")
        {
            value = "none";
        }
        else
        {
            value = "";
        }
        return value;
    }

    //-------------CHECK MAIL
    public static bool CheckHost(string host)
    {
        if (host == "venturasystems")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if value already exists in database
    /// </summary>
    /// <param name="value">value to check for</param>
    /// <param name="table">table to check in</param>
    /// <param name="column">column to check in</param>
    /// <returns>boolean</returns>
    public static bool CheckIfAvaillable(string value, string table, string column)
    {
        string query = string.Format("SELECT 1 FROM {1} WHERE {0} LIKE '%{2}%'", column, table, value);
        DataTable result = new DataTable();
        result = Connection.ExecuteQuery(query);

        if (result.Rows.Count == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static User GetUser(string email)
    {
        string emailtemp = email.Split('@').First();
        string query = string.Format("SELECT Employee_ID FROM Employee WHERE Email LIKE '{0}%'", emailtemp);
        DataTable result = new DataTable();
        result = Connection.ExecuteQuery(query);
        User usr = new User(int.Parse(result.Rows[0].ItemArray[0].ToString()));
        //User usr = new User(1);
        return usr;
    }

    public static bool AllowTrip(DateTime start, DateTime end, int userID)
    {
        if (end < start)
        {
            return false;
        }
        string query = string.Format("select StartDate , EndDate from trip Where Employee_ID = {0}", userID);
        DataTable dt = new DataTable();
        dt = Connection.ExecuteQuery(query);

        List<DateTime> usedDates = new List<DateTime>();
        List<DateTime> requestedTrip = new List<DateTime>();

        //maak een lijst van alle dates tussen start en end
        for (DateTime requestedDate = start; requestedDate <= end; requestedDate = requestedDate.AddDays(1))
        {
            requestedTrip.Add(requestedDate);
        }

        // maak een list van alle dagen dat iemand weg is geweest
        foreach (DataRow row in dt.Rows)
        {
            for (DateTime usedDate = (DateTime)row[0]; usedDate <= (DateTime)row[1]; usedDate = usedDate.AddDays(1))
            {
                usedDates.Add(usedDate);
            }
        }

        //vergelijk alle data 
        foreach (DateTime date in requestedTrip)
        {
            if (usedDates.Contains(date))
            {
                return false;
            }
        }

        return true;
    }

    public static void AddTrip(DateTime start, DateTime end, string reason, string destination, int employee_ID, int compensation_ID)
    {
        string query = string.Format("INSERT INTO trip(StartDate, EndDate, Reason, Destination, Employee_ID, Compensation_ID) VALUES('{0}','{1}','{2}','{3}',{4},{5})", start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"), reason, destination, employee_ID, compensation_ID);
        Connection.ExecuteNonQuery(query);
    }

    public static List<string> Dropdown(string field, string table, string condition)
    {
        string query = string.Format("SELECT {0} FROM {1} WHERE {2}",field,table, condition);
        DataTable dt = Connection.ExecuteQuery(query);
        List<string> stringlist = new List<string>();
        foreach (DataRow row in dt.Rows)
        {            
            stringlist.Add(row[0].ToString());
        }
        return stringlist;
    }

    public static Dictionary<User, int> UserDropdown(int Department_ID, int Employee_ID)
    {
        string query = string.Format("SELECT employee.Employee_ID FROM Employee LEFT JOIN Employee_has_department ON Employee.Employee_ID = Employee_has_department.Employee_ID WHERE Department_ID = {0} and employee.Employee_ID != {1}", Department_ID.ToString(), Employee_ID);

        DataTable dt = Connection.ExecuteQuery(query);
        Dictionary<User, int> usrList = new Dictionary<User, int>();
        foreach (DataRow row in dt.Rows)
        {
            query = string.Format("SELECT COUNT(Trip_ID) FROM trip WHERE Employee_ID = {0} AND status LIKE 'applied'", row[0]);
            DataTable dtbl = Connection.ExecuteQuery(query);
            DataRow rowCount = dtbl.Rows[0];

            if (rowCount != null && int.Parse(rowCount[0].ToString()) > 0)
            {
                User usr = new User(int.Parse(row[0].ToString()));
                usrList.Add(usr, int.Parse(rowCount[0].ToString()));
            }
        }
        return usrList;
    }

    public static List<string[]> GetDisplayData(int employee_id, bool checker, bool payer)
    {
        string fields = "trip.StartDate,trip.EndDate,trip.Destination,trip.Reason,trip.Status,checked.datetime,trip.Comment,compensation.value,payed.DateTime";
        string query = string.Empty;
        List<string[]> data = new List<string[]>();
        if (checker && !payer)
        {
            query = string.Format("SELECT {0} FROM trip LEFT JOIN checked ON trip.Trip_ID = checked.Trip_ID LEFT JOIN compensation ON trip.compensation_ID = compensation.Compensation_ID LEFT JOIN payed ON trip.Trip_ID=payed.Trip_ID WHERE Employee_id = {1} AND status NOT LIKE 'payed' AND status NOT LIKE 'registered'", fields, employee_id);
        }
        else if(checker && payer)
        {
            query = string.Format("SELECT {0} FROM trip LEFT JOIN checked ON trip.Trip_ID = checked.Trip_ID LEFT JOIN compensation ON trip.compensation_ID = compensation.Compensation_ID LEFT JOIN payed ON trip.Trip_ID=payed.Trip_ID WHERE Employee_id = {1} AND status LIKE 'accepted'", fields, employee_id);
        }
        else
        {
            query = string.Format("SELECT {0} FROM trip LEFT JOIN checked ON trip.Trip_ID = checked.Trip_ID LEFT JOIN compensation ON trip.compensation_ID = compensation.Compensation_ID LEFT JOIN payed ON trip.Trip_ID=payed.Trip_ID WHERE Employee_id = {1}", fields, employee_id);
        }
        DataTable result = new DataTable();
        result = Connection.ExecuteQuery(query);

        foreach (DataRow row in result.Rows)
        {
            DateTime startDate = (DateTime)row[0];
            DateTime endDate = (DateTime)row[1];
            string destination = row[2].ToString();
            string reason = row[3].ToString();
            string status = row[4].ToString();
            DateTime lastAlter = new DateTime();
            if (!row.IsNull(5))
            {
                lastAlter = (DateTime)row[5];
                if (!row.IsNull(8))
                {
                    lastAlter = (DateTime)row[8];
                }
            }
            string comments = row[6].ToString();
            int value = int.Parse(row[7].ToString());
            int quarter = (startDate.Month+2)/3;
            int compensation = value * ((int)(endDate - startDate).TotalDays);

            string[] fillData = new string[] { quarter.ToString(), startDate.Date.ToString("dd-MM-yyyy"), endDate.Date.ToString("dd-MM-yyyy"), destination, reason, status, (lastAlter != new DateTime())?lastAlter.Date.ToString("dd-MM-yyyy"):"", compensation.ToString(), comments, value.ToString() };
            data.Add(fillData);
        }
        return data;        
    }

    public static void DeleteTrip(DateTime start, int employee_id)
    {
        string query = string.Format("DELETE FROM trip WHERE StartDate = '{0}' AND Employee_ID = {1}", start.ToString("yyyy-MM-dd"), employee_id);

        Connection.ExecuteNonQuery(query);
        
    }

    public static void acceptTrip(int manager, object employee_id, DateTime startdate, string status)
    {
        int t_id;
        string sd = startdate.ToString("yyyy-MM-dd");
        int manager_id;

        string query = string.Format("select trip_ID from Trip Where employee_ID = {0} AND startDate = '{1}'", employee_id, sd);
        DataTable dt = Connection.ExecuteQuery(query);
        DataRow row = dt.Rows[0];
        t_id = int.Parse(row[0].ToString());

        if (status == "payed")
        {
            query = string.Format("SELECT payer_id FROM financepayer WHERE Employee_ID = {0}", manager);
            dt = Connection.ExecuteQuery(query);
            row = dt.Rows[0];
            manager_id = int.Parse(row[0].ToString());

            query = string.Format("INSERT INTO payed (DateTime, FinancePayer_id, Trip_ID) values ('{0}', {1}, {2})", DateTime.Today.ToString("yyyy-MM-dd"), manager_id, t_id);
            Connection.ExecuteNonQuery(query);
        }
        else if (status == "accepted" || status == "declined")
        {
            query = string.Format("SELECT * FROM checked WHERE Trip_ID = {0}",t_id);
            DataTable dtc = Connection.ExecuteQuery(query);

            if (dtc.Rows.Count != 0)
            {
                query = string.Format("SELECT manager_id FROM manager WHERE Employee_ID = {0}", manager);
                dt = Connection.ExecuteQuery(query);
                row = dt.Rows[0];
                manager_id = int.Parse(row[0].ToString());

                query = string.Format("UPDATE checked SET (DateTime = '{0}', Manager_ID = {1}, Trip_ID = {2})", DateTime.Today.ToString("yyyy-MM-dd"), manager_id, t_id);
                Connection.ExecuteNonQuery(query);
            }
            else
            {        
                query = string.Format("SELECT manager_id FROM manager WHERE Employee_ID = {0}", manager);
                dt = Connection.ExecuteQuery(query);
                row = dt.Rows[0];
                manager_id = int.Parse(row[0].ToString());

                query = string.Format("INSERT INTO checked (DateTime, Manager_ID, Trip_ID) values ('{0}', {1}, {2})", DateTime.Today.ToString("yyyy-MM-dd"), manager_id, t_id);
                Connection.ExecuteNonQuery(query);
            }
        }

        //accept query
        query = string.Format("Update trip set status='{1}' where trip_ID = {0}", t_id, status);
        Connection.ExecuteNonQuery(query);
    }

    public static Dictionary<User,int> PayerDropDown()
    {
        string query = string.Format("SELECT FirstName, LastName, Employee_ID FROM employee");

        DataTable dt = Connection.ExecuteQuery(query);
        Dictionary<User, int> usrList = new Dictionary<User, int>();
        foreach (DataRow row in dt.Rows)
        {
            query = string.Format("SELECT COUNT(Trip_ID) FROM trip WHERE Employee_ID = {0} AND status LIKE 'accepted'",row[2]);
            DataTable dtbl = Connection.ExecuteQuery(query);
            DataRow rowCount = null;
            if (dtbl.Rows.Count != 0)
            {
                rowCount = dtbl.Rows[0];
            }

            if (rowCount != null && int.Parse(rowCount[0].ToString()) > 0)
            {
                User usr = new User(int.Parse(row[2].ToString()));
                usrList.Add(usr, int.Parse(rowCount[0].ToString()));
            }
        }
        return usrList;
    }

    public static void SendMail(int userID, string fName, string lName)
    {
        string sMailServer = "smtp.gmail.com";
        MailMessage MyMail = new MailMessage()
        {
            From = new MailAddress("noreply@venturasystems.com"),
            Subject = "Confirmation of" + fName + " " + lName,
            Body = "Please confirm the account of: " + " " + lName + "\n localhost:51986/confirmRegistration.cshtml?ID=" + userID
        };        
        MyMail.To.Add("m.stoelinga@venturasystems.com");        

        SmtpClient smtp = new SmtpClient(sMailServer, 587);
        smtp.EnableSsl = true;
        NetworkCredential netCre =
                            new NetworkCredential("P3P.NHL@gmail.com", "Groep777");
        smtp.Credentials = netCre;
        smtp.Send(MyMail);
    }


    public static string GetUserByID (int ID)
    {
        string query = string.Format("SELECT FirstName, LastName FROM Employee WHERE Employee_ID = {0}", ID);
        DataTable result = new DataTable();
        result = Connection.ExecuteQuery(query);
        DataRow resultRow = result.Rows[0];
        string usr = resultRow[0].ToString() + " " + resultRow[1].ToString();
        return usr;
    }
    //TODO Add get display data function
}