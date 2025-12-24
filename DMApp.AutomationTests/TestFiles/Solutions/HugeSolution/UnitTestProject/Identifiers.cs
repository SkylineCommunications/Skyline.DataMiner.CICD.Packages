namespace UnitTestProject
{
	using System;
	using System.Collections.Generic;

	public class Identifier
	{
		public Identifier(Random random = null)
		{
			if (random != null)
			{
				Name = $"name {random.Next(10000)}";
				ID = Guid.NewGuid();
			}
		}

		public string Name { get; set; }

		public Guid ID { get; set; }
	}

	public class Identifiers
	{
		public Identifier ServiceIdentifier { get; set; }

		public Identifier ServiceDefinitionIdentifier { get; set; }

		public List<FunctionAndResourceIdentifiers> FunctionAndResourceIdentifiers { get; set; } = new List<FunctionAndResourceIdentifiers>();

		public static Identifiers CreateRandom(Random random = null)
		{
			random = random ?? new Random();

			var identifiers = new Identifiers
			{
				ServiceIdentifier = new Identifier(random),
				ServiceDefinitionIdentifier = new Identifier(random),
				FunctionAndResourceIdentifiers = new List<FunctionAndResourceIdentifiers>
				{
					new FunctionAndResourceIdentifiers (random),
					new FunctionAndResourceIdentifiers (random)
				}
			};

			return identifiers;
		}
	}

	public class FunctionAndResourceIdentifiers
	{
		public FunctionAndResourceIdentifiers(Random random = null)
		{
			if (random != null)
			{
				FunctionIdentifier = new Identifier(random);
				FunctionDefinitionIdentifier = new Identifier(random);
				ResourceIdentifier = new Identifier(random);
			}
		}

		public Identifier FunctionIdentifier { get; set; }

		public Identifier FunctionDefinitionIdentifier { get; set; }

		public Identifier ResourceIdentifier { get; set; }
	}
}
