using Test.Models;

public class CarService
{
    // This list holds the cars you "Liked"
    public List<Car> MatchedCars { get; set; } = new List<Car>();
    public event Action? OnChange;

    public void AddMatch(Car car)
    {
        if (!MatchedCars.Any(c => c.Id == car.Id))
        {
            MatchedCars.Add(car);
            OnChange?.Invoke();
        }
    }
}