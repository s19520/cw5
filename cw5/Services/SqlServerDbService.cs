using cw5.DTOs.Requests;
using cw5.DTOs.Responses;
using cw5.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace cw5.Services
{
    public class SqlServerDbService : IStudentsDbService
    {
        private const string ConString = "Data Source=DESKTOP-RSTT48M\\SQLEXPRESS;Initial Catalog=apbd;Integrated Security=True";


        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest newStudent)
        {
            var st = new EnrollStudentRequest();
            st.FirstName = newStudent.FirstName;
            st.LastName = newStudent.LastName;
            st.IndexNumber = newStudent.IndexNumber;
            st.Studies = newStudent.Studies;
            st.BirthDate = newStudent.BirthDate;

            var resp = new EnrollStudentResponse();
            var date = new DateTime();
            using (var client = new SqlConnection(ConString))
            using (var com = new SqlCommand())
            {

                com.Connection = client;
                client.Open();
                var tran = client.BeginTransaction();
                com.Transaction = tran;
                try
                {
                    //Czy istnieja studia o takiej nazwie?
                    com.CommandText = "select IdStudy from studies where name =@name";
                    com.Parameters.AddWithValue("name", st.Studies);
                    var dr1 = com.ExecuteReader();

                    //jeśli nie, zwroc blad
                    if (!dr1.Read())
                    {
                        dr1.Close();
                        tran.Rollback();
                        throw new ArgumentException("Improper Study name");

                    }
                    //jesli tak, zapisz ID
                    int idStud = (int)dr1["IdStudy"];
                    dr1.Close();
                    resp.idStudy = idStud;

                    //znajdz enrollmenty dla study ID i 1 semestru
                    com.CommandText = "select IdEnrollment,StartDate from enrollment where IdStudy=@id and semester=1";
                    com.Parameters.AddWithValue("id", idStud);
                    var dr2 = com.ExecuteReader();

                    //jesli nie istnieja, dopisz
                    if (!dr2.Read())
                    {
                        int next = 1;
                        using (var client1 = new SqlConnection(ConString))
                        using (var com1 = new SqlCommand())
                        {

                            com1.Connection = client1;
                            client1.Open();
                            com1.CommandText = "SELECT IdEnrollment FROM enrollment WHERE IdEnrollment = (SELECT MAX(IdEnrollment) FROM enrollment)";
                            var dr3 = com1.ExecuteReader();
                            while (dr3.Read())
                            {
                                next = (int)dr3["IdEnrollment"] + 1;
                                resp.IdEnrollment = next;
                            }
                            dr3.Close();

                            date = DateTime.Now.Date;
                            com1.CommandText = "insert into enrollment(Semester, IdStudy, StartDate,IdEnrollment) values(@sem, @ids, @start," + next + ")";
                            com1.Parameters.AddWithValue("sem", 1);
                            com1.Parameters.AddWithValue("ids", idStud);
                            com1.Parameters.AddWithValue("start", date);


                            var dr4 = com1.ExecuteNonQuery();

                        }
                        dr2.Close();
                    }
                    else
                    {
                        resp.IdEnrollment = (int)dr2["IdEnrollment"];
                        date = (DateTime)dr2["StartDate"];

                        //jesli istnieje enrollment, wez najnowszy
                        while (dr2.Read())
                        {
                            DateTime dateTime = (DateTime)dr2["StartDate"];
                            if (DateTime.Compare(dateTime, date) > 0)
                            {
                                date = (DateTime)dr2["StartDate"];
                                resp.IdEnrollment = (int)dr2["IdEnrollment"];

                            }

                        }
                        dr2.Close();
                    }
                    //sprawdz, czy index number juz nie istnieje
                    com.CommandText = "select 1 from student where IndexNumber=@index";
                    com.Parameters.AddWithValue("index", st.IndexNumber);
                    var dr5 = com.ExecuteReader();

                    //jesli nie, dodaj studenta
                    if (!dr5.Read())
                    {
                        dr5.Close();

                        com.CommandText = "insert into student values(@indexnumber, @first, @last, @birth, @enr)";
                        com.Parameters.AddWithValue("indexnumber", st.IndexNumber);
                        com.Parameters.AddWithValue("first", st.FirstName);
                        com.Parameters.AddWithValue("last", st.LastName);
                        com.Parameters.AddWithValue("birth", st.BirthDate);
                        com.Parameters.AddWithValue("enr", resp.IdEnrollment);
                        var dr6 = com.ExecuteNonQuery();
                        tran.Commit();

                        resp.Semester = 1;
                        resp.StartDate = date;

                        return resp;

                    }
                    else throw new ArgumentException("Index Number already exists");

                }
                catch (SqlException e)
                {
                    tran.Rollback();
                    throw new ArgumentException(e.Message);
                }
            }
        }

        public PromoteStudentsResponse PromoteStudent(PromoteStudentsRequest promotion)
        {
            var pr = new PromoteStudentsRequest();
            pr.Studies = promotion.Studies;
            pr.Semester = promotion.Semester;
            using (var client = new SqlConnection(ConString))
            using (var com = new SqlCommand())
            {
                com.Connection = client;
                client.Open();
                var tran = client.BeginTransaction();
                com.Transaction = tran;
                PromoteStudentsResponse resp = new PromoteStudentsResponse();
                com.CommandText = "EXEC PromoteStudents '" + pr.Studies + "', " + pr.Semester + " WITH RESULT SETS(( IdEnrollment INT,IDStudy INT,Semester INT,StartDate DateTime)) ";


                /*used procedure:
CREATE PROCEDURE PromoteStudents @Studies NVARCHAR(100), @Semester INT
    AS
    BEGIN
	DECLARE @IdStudies INT = (SELECT IdStudy FROM Studies WHERE NAme=@Studies);
	IF @IdStudies IS NULL
	BEGIN
	RETURN;
	
	END

	DECLARE @StartDate DATETIME = (SELECT MAX(startdate) FROM enrollment where IdStudy=@IdStudies and Semester =@Semester)

	DECLARE @IdEnrollment INT  = (select IdEnrollment from enrollment where IdStudy=@IdStudies and Semester =@Semester and startdate = @StartDate)

	IF @IdEnrollment IS NULL
	BEGIN
	RETURN;
	
	END

	DECLARE @NextSemester INT = (select IdEnrollment from enrollment where IdStudy=@IdStudies and Semester =@semester+1)
	IF @NextSemester IS NULL
	BEGIN
	DECLARE @Max INT = (SELECT MAX(IdEnrollment) FROM enrollment)
	DECLARE @NextId INT = @Max +1
	insert into enrollment values(@NextId, @semester+1, @IdStudies, CURRENT_TIMESTAMP)
	UPDATE student SET IdEnrollment=@NextId WHERE IdEnrollment=@IdEnrollment
	SELECT * FROM enrollment WHERE  IdEnrollment=@NextId;
	END
	ELSE

	UPDATE student SET IdEnrollment=@NextSemester WHERE IdEnrollment=@IdEnrollment
	SELECT * FROM enrollment WHERE  IdEnrollment=@NextSemester;
	RETURN;
END;

                 */
                var dr = com.ExecuteReader();
                while (dr.Read())
                {
                    resp.IdEnrollment = (int)dr["IdEnrollment"];
                    resp.idStudy = (int)dr["IDStudy"];
                    resp.Semester = (int)dr["Semester"];
                    resp.StartDate = (DateTime)dr["StartDate"];
                }
                dr.Close();

                tran.Commit();
                return (resp);

            }
        }

        Student IStudentsDbService.GetStudent(string index)
        {
            Student resp = new Student();
            using (var client = new SqlConnection(ConString))
            using (var com = new SqlCommand())
            {
                com.Connection = client;
                client.Open();
                com.CommandText = "SELECT * FROM Student Where IndexNumber = '" + index + "'";
                var dr = com.ExecuteReader();
                if (!dr.Read()) return null;
                do
                {
                    resp.FirstName = (string)dr["FirstName"];
                    resp.LastName = (string)dr["LastName"];
                    resp.BirthDate = (DateTime)dr["BirthDate"];
                    resp.IndexNumber = (string)dr["IndexNumber"];
                    resp.EnrollmentId = (int)dr["IdEnrollment"];

                } while (dr.Read());
                return resp;
            }
        }
        List<Student> IStudentsDbService.GetStudents()
        {
            var list = new List<Student>();
            using (var client = new SqlConnection(ConString))
            using (var com = new SqlCommand())
            {
                com.Connection = client;
                client.Open();
                com.CommandText = "SELECT * FROM Student";
                var dr = com.ExecuteReader();
                while (dr.Read())
                {
                    var student = new Student();
                    student.FirstName = (string)dr["FirstName"];
                    student.LastName = (string)dr["LastName"];
                    student.BirthDate = (DateTime)dr["BirthDate"];
                    student.IndexNumber = (string)dr["IndexNumber"];
                    student.EnrollmentId = (int)dr["IdEnrollment"];
                    list.Add(student);

                }
            }
            return list;
        }
    }
}
            
        
