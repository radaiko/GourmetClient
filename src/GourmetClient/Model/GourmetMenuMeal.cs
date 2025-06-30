namespace GourmetClient.Model
{
	using System;

	public class GourmetMenuMeal
	{
		public GourmetMenuMeal(string positionId, string name, string description)
		{
            ProductId = positionId ?? throw new ArgumentNullException(nameof(positionId));
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Description = description ?? throw new ArgumentNullException(nameof(description));
		}

		public string ProductId { get; }

		public string Name { get; }

		public string Description { get; }
	}
}
