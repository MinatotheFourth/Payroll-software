/*
 * 
 * This program is a simple payroll software that reads data from a file, and calculates the monthly payment for each employee the file reads. 
 * Employees are sorted by position via admin and manager which means their pay will differ.
 * The employee source data (staff.txt) file is located in the CSProject/CSProject/bin/Debug folder.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
using System.IO;

namespace CSProject
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Staff> myStaff = new List<Staff>();
            FileReader fr = new FileReader();
            int month = 0, year = 0;

            while(year == 0)
            {
                Write("\nPlease enter the year: ");
                string userInput = ReadLine();

                try
                {
                    year = Int32.Parse(userInput);   
                }

                catch(FormatException)
                {
                    WriteLine("Invalid input! Please enter the year!");
                }
            }

            while (month == 0)
            {
                Write("\nPlease enter the month as an integer: ");
                string userInput = ReadLine();

                try
                {
                   month = Int32.Parse(userInput);
                    if (month < 1 || month > 12)
                    {
                        WriteLine("Invalid input! Please enter the month!");
                        month = 0;
                    }
                }

                catch (FormatException)
                {
                    WriteLine("Invalid input! Please enter the month as an integer!");
                }
            }

            myStaff = fr.ReadFile();

            for(int i = 0; i < myStaff.Count; i++)
            {
                try
                {
                    WriteLine("\nEnter the hours worked by: {0}", myStaff[i].NameOfStaff + " as an integer.");
                    string input = ReadLine();
                    myStaff[i].HoursWorked = Int32.Parse(input);
                    myStaff[i].CalculatePay();
                    Write(myStaff[i].ToString());
                }

                catch(Exception e)
                {
                    WriteLine(e.Message);
                    i--;
                }
            }

            PaySlip ps = new PaySlip(month, year);
            ps.GeneratePaySlip(myStaff);
            ps.GenerateSummary(myStaff);

            WriteLine("\n\nPaySlips for each employee have been generated as a .txt file in the " +
                "\"CSProject\\CSProject\\bin\\Debug\" folder " +
                "along with a summary for those who worked under 10 hours this month. \n\nPress any key to close this console.");

            ReadKey();
        }
    }

    class Staff
    {
        private float hourlyRate;
        private int hworked;

        public float TotalPay { get; protected set; }
        public float BasicPay { get; private set; }
        public string NameOfStaff { get; private set; }

        public int HoursWorked
        {
            get
            {
                return hworked;
            }

            set
            {
                if (value > 0)
                    hworked = value;
                else
                    hworked = 0;
            }
        }

        public Staff(string Name, float rate)
        {
            NameOfStaff = Name;
            hourlyRate = rate;
        }

        public virtual void CalculatePay()
        {
            WriteLine("Calculating Pay...");
            BasicPay = (hworked * hourlyRate);
            TotalPay = BasicPay;
        }

        public override string ToString()
        {
            return "Name of Staff: " + NameOfStaff + "\nHourly Rate: $"
                + hourlyRate + "\nHours Worked: " + hworked + "\nBasic Pay: $" + BasicPay
                + "\nTotal Pay: $" + TotalPay;
        }
    }


    class Manager : Staff
    {
        private const float managerHourlyRate = 50;
        public int Allowance { get; private set; }

        public Manager(string name) : base(name, managerHourlyRate)
        {

        }

        public override void CalculatePay()
        {
            base.CalculatePay();
            Allowance = 1000;

            if (HoursWorked > 160)
                TotalPay = TotalPay + Allowance;
            else
                Allowance = 0;
        }

        public override string ToString()
        {
            return "Name of Staff: " + NameOfStaff + "\nHourly Rate: $"
                + managerHourlyRate + "\nHours Worked: " + HoursWorked + "\nBasic Pay: $" + BasicPay
                + "\nTotal Pay: $" + TotalPay;
        }
    }

    class Admin : Staff
    {
        private const float overtimeRate = 15.5f;
        private const float adminHourlyRate = 30;

        public float Overtime { get; private set; }

        public Admin(string name) : base(name, adminHourlyRate)
        {

        }

        public override void CalculatePay()
        {
            base.CalculatePay();

            if (HoursWorked > 160)
                Overtime = overtimeRate * (HoursWorked - 160);

            TotalPay = TotalPay + Overtime;
        }

        public override string ToString()
        {
            return "Name of Staff: " + NameOfStaff + "\nHourly Rate: "
                + adminHourlyRate + "\nHours Worked: " + HoursWorked + "\nBasic Pay: $" + BasicPay
                + "\nTotal Pay: $" + TotalPay + "\nOvertime: $" + Overtime;
        }
    }

    class FileReader
    {
        public List<Staff> ReadFile()
        {
            List<Staff> myStaff = new List<Staff>();
            string[] result = new string[2];
            string path = "staff.txt";
            string[] separator = {", "};

            if (File.Exists(path))
                using (StreamReader sr = new StreamReader(path))
                {
                    while(sr.EndOfStream != true)
                    {
                        result = sr.ReadLine().Split(separator, StringSplitOptions.RemoveEmptyEntries);//The file's lines are read as "Employee Name, Position" (e.g David Gonzales, Admin || Riley Fowle, Manager)"
                        if (result[1] == "Manager")
                            myStaff.Add(new Manager(result[0]));
                        else if (result[1] == "Admin")
                                myStaff.Add(new Admin(result[0]));

                    }
                    sr.Close();
                }
            else
                WriteLine("File not found!");

            return myStaff;
        }
        
    }

    class PaySlip
    {
        private int month, year;

        public PaySlip(int payMonth, int payYear)
        {
            month = payMonth;
            year = payYear;
        }


        enum MonthsOfYear
        {
            JAN = 1, FEB, MAR, APRIL, MAY, JUNE, JULY, AUG, SEP, OCT, NOV, DEC
        }

        public void GeneratePaySlip(List<Staff> myStaff)
        {
            

            foreach (Staff f in myStaff)
            {
                string path = (string)f.NameOfStaff + ".txt";

                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine("PAYSLIP FOR {0} {1}", (MonthsOfYear)month, year);
                    sw.WriteLine("===============================================");
                    sw.WriteLine("Name of Staff:{0} ", f.NameOfStaff);
                    sw.WriteLine("Hours worked: {0}", f.HoursWorked);
                    sw.WriteLine("");
                    sw.WriteLine("Basic Pay: ${0}", f.BasicPay);

                    if(f.GetType() == typeof(Manager))
                        sw.WriteLine("Allowance: {0:C}", ((Manager)f).Allowance);

                    else if(f.GetType() == typeof(Admin))
                        sw.WriteLine("Overtime: {0:C}", ((Admin)f).Overtime);

                    sw.WriteLine("");
                    sw.WriteLine("===============================================");
                    sw.WriteLine("Total Pay: ${0}", f.TotalPay);
                    sw.WriteLine("===============================================");

                    sw.Close();
                }
            }
        }

        public void GenerateSummary(List<Staff> myStaff )
        {
            var result = from staff in myStaff where (staff.HoursWorked < 10) orderby staff.HoursWorked ascending select new { staff.NameOfStaff, staff.HoursWorked };
            string path = "summary.txt";

            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("Staff with less than 10 working hours");
                sw.WriteLine("");
                foreach (var f in result)
                {
                    sw.WriteLine("");
                    sw.WriteLine("Name of Staff: {0} ", f.NameOfStaff, f.HoursWorked);
                    sw.WriteLine("Hours Worked: {0} ", f.HoursWorked);
                    sw.WriteLine("===============================================");
                }
                sw.Close();
            }
        }

        public override string ToString()
        {
            return "Year: " + year + "Month: " + month;
        }
    }
}

