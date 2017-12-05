// <copyright file = "connection.cs" company="Ventura Systems">
// Copyright (c) Ventura Systems. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Summary description for Connection
/// </summary>
public class Connection
{
    /// <summary>
    /// create a variable from class connection which controls the database manipulations. Use connect as container for this class
    /// </summary>
    private static MySqlConnection _DBConnection;

    private static MySqlConnection DBConnection
    {
        get
        {
            if (_DBConnection == null)
            {
                _DBConnection = new MySqlConnection("Server=localhost; database=venturatrips; UID=venturatrips; password=Ventura01");
            }
            return _DBConnection;
        }
    }

    ////methodes

    /// <summary>
    /// return result of the query
    /// </summary>
    /// <param name="query">string of query to execute</param>
    /// <returns>a DataTable with the result of the Query</returns>
    public static DataTable ExecuteQuery(string query)
    {
        MySqlDataAdapter ad;
        DataTable dt = new DataTable();

        try
        {
            MySqlCommand cmd;
            DBConnection.Open();
            cmd = DBConnection.CreateCommand();
            cmd.CommandText = query;
            ad = new MySqlDataAdapter(cmd);
            ad.Fill(dt);
        }
        catch (MySqlException ex)
        {
            System.Console.WriteLine(ex.Message);
        }

        if (DBConnection != null && DBConnection.State == ConnectionState.Open)
        {
            DBConnection.Close();
        }

        return dt;
    }

    /// <summary>
    /// manipulation of the database
    /// </summary>
    /// <param name="query">string of NonQuery to execute</param>
    public static void ExecuteNonQuery(string query)
    {
        try
        {
            MySqlCommand cmd;
            DBConnection.Open();
            cmd = DBConnection.CreateCommand();
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
        }
        catch (MySqlException ex)
        {
            ////Add your exception code here.
            System.Console.WriteLine(ex.Message);
        }

        if (DBConnection != null && DBConnection.State == ConnectionState.Open)
        {
            DBConnection.Close();
        }
    }

    public static string Connectionstate()
    {
        if(DBConnection.State == ConnectionState.Open)
        {
            string query = "select * from employee";
            DataTable result = new DataTable();

            result = ExecuteQuery(query);

            string firstRow = result.Rows[0].ItemArray[1].ToString();

            return "Thnx voor je hulp, " + firstRow;
        }
        else
        {
            return "The connection is dead.";
        }
    }

    public static string Hash(string input)
    {
        using (SHA1Managed sha1 = new SHA1Managed())
        {
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(hash.Length * 2);

            foreach (byte b in hash)
            {
                // can be "x2" if you want lowercase
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}