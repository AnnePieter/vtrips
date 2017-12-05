using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

/// <summary>
/// Summary description for User
/// </summary>
public class User
{
    //fictief ingevuld moet met constructor!!
    public string FirstName;
    public string LastName;
    public int EmployeeID;
    public int RoleID;
    public int EmployeeNumber;
    public int PayNumber;
    public int department_ID;
    public bool manager = false;
    public bool payer = false;


    public User(int EmplID)
    {
        EmployeeID = EmplID;

        string query = "SELECT * FROM Employee WHERE Employee_ID = "+ EmplID;
        DataTable dt = new DataTable();
        dt = Connection.ExecuteQuery(query);
        try
        {
            FirstName = dt.Rows[0].ItemArray[1].ToString();
            LastName = dt.Rows[0].ItemArray[2].ToString();
            RoleID = int.Parse(dt.Rows[0].ItemArray[8].ToString());
            EmployeeNumber = int.Parse(dt.Rows[0].ItemArray[3].ToString());
            PayNumber = int.Parse(dt.Rows[0].ItemArray[4].ToString());

            query = string.Format("SELECT department_ID FROM Employee_has_Department WHERE Employee_ID = {0}", EmplID);
            dt = Connection.ExecuteQuery(query);

            department_ID = int.Parse(dt.Rows[0].ItemArray[0].ToString());
            
        }
        catch(Exception e)
        { 
        }
        Exists();
    }

    private void Exists()
    {
        string query = string.Format("SELECT * FROM manager WHERE employee_ID = {0}", EmployeeID);
        DataTable dt = new DataTable();
        dt = Connection.ExecuteQuery(query);

        if (dt.Rows.Count > 0)
        {
            manager = true;
        }

        query = string.Format("SELECT * FROM financepayer WHERE employee_ID = {0}", EmployeeID);
        dt = Connection.ExecuteQuery(query);

        if (dt.Rows.Count > 0)
        {
            payer = true;
        }

    }

    public void ConfirmAccount (int empNo, int payNo, int role)
    {
        string query = string.Format("UPDATE employee SET  EmployeeNumber = {0}, PayNumber= {1}, Role_idRole = {3} WHERE Employee_ID = {2};", empNo, payNo, this.EmployeeID, role);
        Connection.ExecuteNonQuery(query);

        //manager and payer
        if (role == 3)
        {
            query = string.Format("INSERT INTO manager (Employee_ID, department_ID) VALUES ({0},{1})", this.EmployeeID, this.department_ID);
            Connection.ExecuteNonQuery(query);

            query = string.Format("INSERT INTO financepayer (Employee_ID) VALUES ({0})", this.EmployeeID);
            Connection.ExecuteNonQuery(query);
        }
        //manager
        else if (role == 4)
        {
            query = string.Format("INSERT INTO manager (Employee_ID, department_ID) VALUES ({0},{1})", this.EmployeeID, this.department_ID);
            Connection.ExecuteNonQuery(query);
        }
        //payer
        else if (role == 5)
        {
            query = string.Format("INSERT INTO financepayer (Employee_ID) VALUES ({0})", this.EmployeeID);
            Connection.ExecuteNonQuery(query);
        }
    }
}