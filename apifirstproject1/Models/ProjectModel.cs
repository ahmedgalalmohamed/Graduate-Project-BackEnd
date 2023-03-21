namespace Graduate_Project_BackEnd.Models
{
  public class ProjectModel
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Desciption { get; set; }

    public TeamModel Team { get; set; }
    public int TeamID { get; set; }
  }
}
