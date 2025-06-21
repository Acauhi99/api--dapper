using api__dapper.domain.models;

using Microsoft.Data.Sqlite;

using NanoidDotNet;

namespace api__dapper.infra;

public static class DatabaseConfig
{
  public static void InitializeDatabase(IConfiguration configuration)
  {
    // Garantir que o diretório existe
    var dbPath = configuration.GetConnectionString("DefaultConnection");
    if (dbPath != null && dbPath.Contains("Data Source="))
    {
      var filePath = dbPath.Replace("Data Source=", "").Split(';')[0];
      var directory = Path.GetDirectoryName(filePath);
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
      {
        Directory.CreateDirectory(directory);
      }
    }

    var connectionString = configuration.GetConnectionString("DefaultConnection");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    CreateTables(connection);

    // Verifica se o banco já foi populado (checando se há usuários)
    var checkCommand = connection.CreateCommand();
    checkCommand.CommandText = "SELECT COUNT(*) FROM Users";
    var userCount = Convert.ToInt32(checkCommand.ExecuteScalar());

    if (userCount == 0)
    {
      // Popula o banco com dados iniciais
      SeedDatabase(connection);
    }
  }

  private static void CreateTables(SqliteConnection connection)
  {
    var command = connection.CreateCommand();
    command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Email TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL,
                Role TEXT NOT NULL DEFAULT 'User',
                CreatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Services (
                Id TEXT PRIMARY KEY,
                Key TEXT NOT NULL UNIQUE,
                Title TEXT NOT NULL,
                Description TEXT NOT NULL,
                FeaturesTitle TEXT NOT NULL,
                DetailsTitle TEXT NOT NULL,
                PackagesTitle TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ServiceFeatures (
                Id TEXT PRIMARY KEY,
                ServiceId TEXT NOT NULL,
                Text TEXT NOT NULL,
                [Order] INTEGER NOT NULL,
                FOREIGN KEY (ServiceId) REFERENCES Services (Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS ServiceDetails (
                Id TEXT PRIMARY KEY,
                ServiceId TEXT NOT NULL,
                Text TEXT NOT NULL,
                [Order] INTEGER NOT NULL,
                FOREIGN KEY (ServiceId) REFERENCES Services (Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Packages (
                Id TEXT PRIMARY KEY,
                ServiceId TEXT NOT NULL,
                Name TEXT NOT NULL,
                Price TEXT NOT NULL,
                IsPopular INTEGER NOT NULL DEFAULT 0,
                [Order] INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL,
                FOREIGN KEY (ServiceId) REFERENCES Services (Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Sells (
                Id TEXT PRIMARY KEY,
                UserId TEXT NOT NULL,
                ServiceId TEXT NOT NULL,
                PackageId TEXT NOT NULL,
                Amount REAL NOT NULL,
                Status INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                CompletedAt TEXT NULL,
                PaymentMethod TEXT NOT NULL DEFAULT 'PIX',
                FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE,
                FOREIGN KEY (ServiceId) REFERENCES Services (Id) ON DELETE CASCADE,
                FOREIGN KEY (PackageId) REFERENCES Packages (Id) ON DELETE CASCADE
            )";

    command.ExecuteNonQuery();
  }

  private static void SeedDatabase(SqliteConnection connection)
  {
    // Inserir usuários (admin e user comum)
    SeedUsers(connection);

    // Inserir serviços
    SeedServices(connection);

    // Inserir vendas
    SeedSells(connection);
  }

  private static void SeedUsers(SqliteConnection connection)
  {
    using var command = connection.CreateCommand();

    // Admin
    string adminId = Nanoid.Generate(size: 12);
    string adminPassword = BCrypt.Net.BCrypt.HashPassword("admin123");

    command.CommandText = @"
        INSERT INTO Users (Id, Name, Email, Password, Role, CreatedAt)
        VALUES (@Id, @Name, @Email, @Password, @Role, @CreatedAt)";

    command.Parameters.AddWithValue("@Id", adminId);
    command.Parameters.AddWithValue("@Name", "Administrator");
    command.Parameters.AddWithValue("@Email", "admin@example.com");
    command.Parameters.AddWithValue("@Password", adminPassword);
    command.Parameters.AddWithValue("@Role", UserRoles.Admin.ToString());
    command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("o"));

    command.ExecuteNonQuery();
    command.Parameters.Clear();

    // Usuário comum
    string userId = Nanoid.Generate(size: 12);
    string userPassword = BCrypt.Net.BCrypt.HashPassword("user123");

    command.CommandText = @"
        INSERT INTO Users (Id, Name, Email, Password, Role, CreatedAt)
        VALUES (@Id, @Name, @Email, @Password, @Role, @CreatedAt)";

    command.Parameters.AddWithValue("@Id", userId);
    command.Parameters.AddWithValue("@Name", "João Silva");
    command.Parameters.AddWithValue("@Email", "joao@example.com");
    command.Parameters.AddWithValue("@Password", userPassword);
    command.Parameters.AddWithValue("@Role", UserRoles.User.ToString());
    command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("o"));

    command.ExecuteNonQuery();
  }

  private static void SeedServices(SqliteConnection connection)
  {
    // Serviço de Gold
    var goldId = InsertService(connection, "gold", "Serviços de Gold",
        "Obtenha ouro rapidamente de forma segura e garantida. Entregas rápidas em todos os servidores.",
        "Por que escolher nosso serviço de Gold?", "Como funciona?", "Escolha seu pacote");

    // Features do Gold
    InsertServiceFeature(connection, goldId, "Entrega garantida em até 24 horas", 0);
    InsertServiceFeature(connection, goldId, "Métodos 100% seguros, sem risco para sua conta", 1);
    InsertServiceFeature(connection, goldId, "Disponível para todos os servidores", 2);

    // Detalhes do Gold
    InsertServiceDetail(connection, goldId, "1. Escolha a quantidade de gold desejada", 0);
    InsertServiceDetail(connection, goldId, "2. Forneça as informações de seu personagem e servidor", 1);
    InsertServiceDetail(connection, goldId, "3. Realize o pagamento", 2);
    InsertServiceDetail(connection, goldId, "4. Aguarde o contato de nossa equipe via Discord", 3);
    InsertServiceDetail(connection, goldId, "5. Receba seu gold no jogo!", 4);

    // Pacotes de Gold
    var goldPackage1 = InsertPackage(connection, goldId, "10.000 Gold", "R$ 19,90", false, 0);
    var goldPackage2 = InsertPackage(connection, goldId, "100.000 Gold", "R$ 149,90", true, 1);
    var goldPackage3 = InsertPackage(connection, goldId, "500.000 Gold", "R$ 599,90", false, 2);

    // Serviço de Dungeons
    var dungeonsId = InsertService(connection, "dungeons", "Mythic+ Dungeons",
        "Domine as masmorras mais desafiadoras com nossa equipe experiente. Garantimos completar suas dungeons com chaves altas rapidamente.",
        "O que nosso serviço de Mythic+ oferece?", "Como funciona?", "Escolha seu pacote");

    // Features de Dungeons
    InsertServiceFeature(connection, dungeonsId, "Runs rápidas com garantia de conclusão no tempo", 0);
    InsertServiceFeature(connection, dungeonsId, "Equipe composta por jogadores Gladiator/Elite com anos de experiência", 1);
    InsertServiceFeature(connection, dungeonsId, "Loot garantido direcionado para seu personagem", 2);
    InsertServiceFeature(connection, dungeonsId, "Streams privadas para você acompanhar a run", 3);

    // Detalhes de Dungeons
    InsertServiceDetail(connection, dungeonsId, "1. Escolha o nível da chave e a quantidade de runs", 0);
    InsertServiceDetail(connection, dungeonsId, "2. Selecione as masmorras específicas ou deixe-nos escolher as mais rápidas", 1);
    InsertServiceDetail(connection, dungeonsId, "3. Informe detalhes do seu personagem e preferências de loot", 2);
    InsertServiceDetail(connection, dungeonsId, "4. Realize o pagamento", 3);
    InsertServiceDetail(connection, dungeonsId, "5. Agende com nossa equipe e receba seu boost!", 4);

    // Pacotes de Dungeons
    var dungeonsPackage1 = InsertPackage(connection, dungeonsId, "Mythic+10 (Única)", "R$ 49,90", false, 0);
    var dungeonsPackage2 = InsertPackage(connection, dungeonsId, "Pacote 4x Mythic+15", "R$ 189,90", true, 1);
    var dungeonsPackage3 = InsertPackage(connection, dungeonsId, "Mythic+20 (Única)", "R$ 99,90", false, 2);

    // Serviço de Raids
    var raidsId = InsertService(connection, "raids", "Raid Boosting",
        "Conquiste as raids mais desafiadoras e garanta os melhores equipamentos com nossa equipe de elite. Do Normal ao Mítico, nós te levamos ao topo.",
        "Por que escolher nosso boost de raids?", "Raids disponíveis:", "Escolha seu pacote");

    // Features de Raids
    InsertServiceFeature(connection, raidsId, "Equipe composta por raiders de guilds de top mundial", 0);
    InsertServiceFeature(connection, raidsId, "Garantia de loot específico para seu personagem", 1);
    InsertServiceFeature(connection, raidsId, "Disponibilidade para todas as dificuldades: Normal, Heroico e Mítico", 2);
    InsertServiceFeature(connection, raidsId, "Conquistas e montarias raras garantidas", 3);

    // Detalhes de Raids
    InsertServiceDetail(connection, raidsId, "Aberrus, o Crisol Sombreado (Última temporada)", 0);
    InsertServiceDetail(connection, raidsId, "Cofre das Encarnações", 1);
    InsertServiceDetail(connection, raidsId, "Sepulcro dos Fundadores", 2);
    InsertServiceDetail(connection, raidsId, "Outras raids sob consulta", 3);

    // Pacotes de Raids
    var raidsPackage1 = InsertPackage(connection, raidsId, "Full Run Normal", "R$ 89,90", false, 0);
    var raidsPackage2 = InsertPackage(connection, raidsId, "Full Run Heroico", "R$ 199,90", true, 1);
    var raidsPackage3 = InsertPackage(connection, raidsId, "Boss Específico Mítico", "R$ 129,90", false, 2);
    var raidsPackage4 = InsertPackage(connection, raidsId, "Full Run Mítico", "R$ 599,90", false, 3);

    // Serviço de PVP
    var pvpId = InsertService(connection, "pvp", "Serviços de PvP",
        "Alcance ratings elevados, conquiste títulos exclusivos e obtenha equipamentos PvP de elite com nossos serviços profissionais de arena e battleground.",
        "Eleve seu jogo PvP com nossos serviços", "Nossos diferenciais", "Escolha seu pacote");

    // Features de PVP
    InsertServiceFeature(connection, pvpId, "Jogadores Gladiadores e R1 em nosso time", 0);
    InsertServiceFeature(connection, pvpId, "Disponível para 2v2, 3v3 e RBGs", 1);
    InsertServiceFeature(connection, pvpId, "Conquistas como \"Gladiador\" e \"Elite\" garantidas", 2);
    InsertServiceFeature(connection, pvpId, "Opções para jogabilidade própria (coaching) ou pilotagem", 3);

    // Detalhes de PVP
    InsertServiceDetail(connection, pvpId, "Oferecemos gravações das partidas para você estudar e aprender com os melhores jogadores. Também disponibilizamos sessões de coaching para melhorar suas habilidades PvP.", 0);
    InsertServiceDetail(connection, pvpId, "Todos os boosts são realizados manualmente, sem uso de programas ou trapaças, garantindo total segurança para sua conta.", 1);

    // Pacotes de PVP
    var pvpPackage1 = InsertPackage(connection, pvpId, "1800 Rating (2v2 ou 3v3)", "R$ 149,90", false, 0);
    var pvpPackage2 = InsertPackage(connection, pvpId, "2100 Rating (Elite)", "R$ 299,90", true, 1);
    var pvpPackage3 = InsertPackage(connection, pvpId, "2400+ (Gladiador)", "R$ 499,90", false, 2);
    var pvpPackage4 = InsertPackage(connection, pvpId, "RBG 1800+ Rating", "R$ 249,90", false, 3);

    // Serviço de Itens
    var itensId = InsertService(connection, "itens", "Itens Especiais",
        "Obtenha os itens mais raros e cobiçados do jogo. Armas legendárias, montarias raras, transmogs exclusivos e muito mais.",
        "Categorias de Itens Disponíveis", "Nosso processo de farm", "Itens em Destaque");

    // Features de Itens
    InsertServiceFeature(connection, itensId, "Montarias raras com chance de drop baixíssima", 0);
    InsertServiceFeature(connection, itensId, "Transmogs exclusivos e itens cosméticos", 1);
    InsertServiceFeature(connection, itensId, "Armas e equipamentos lendários", 2);
    InsertServiceFeature(connection, itensId, "Pets de batalha raros", 3);
    InsertServiceFeature(connection, itensId, "Itens de colecionador", 4);

    // Detalhes de Itens
    InsertServiceDetail(connection, itensId, "Utilizamos equipes especializadas que trabalham incansavelmente para obter os itens mais raros do jogo. Alguns itens podem levar dias ou semanas para serem adquiridos devido à sua raridade.", 0);
    InsertServiceDetail(connection, itensId, "Todos os itens são obtidos de forma legítima, seguindo as regras do jogo, garantindo a segurança de sua conta.", 1);

    // Pacotes de Itens
    var itensPackage1 = InsertPackage(connection, itensId, "Montaria Rara - Rédeas do Raptor Zandalari", "R$ 199,90", false, 0);
    var itensPackage2 = InsertPackage(connection, itensId, "Montaria - Cabeça de Al'ar", "R$ 349,90", true, 1);
    var itensPackage3 = InsertPackage(connection, itensId, "Set de Transmog - Paladino T3", "R$ 299,90", false, 2);
    var itensPackage4 = InsertPackage(connection, itensId, "Pacote de 3 Pets Raros", "R$ 89,90", false, 3);
  }

  private static string InsertService(SqliteConnection connection, string key, string title, string description,
      string featuresTitle, string detailsTitle, string packagesTitle)
  {
    var serviceId = Nanoid.Generate(size: 12);
    var now = DateTime.UtcNow.ToString("o");

    using var command = connection.CreateCommand();
    command.CommandText = @"
        INSERT INTO Services (Id, Key, Title, Description, FeaturesTitle, DetailsTitle, PackagesTitle, CreatedAt, UpdatedAt)
        VALUES (@Id, @Key, @Title, @Description, @FeaturesTitle, @DetailsTitle, @PackagesTitle, @CreatedAt, @UpdatedAt)";

    command.Parameters.AddWithValue("@Id", serviceId);
    command.Parameters.AddWithValue("@Key", key);
    command.Parameters.AddWithValue("@Title", title);
    command.Parameters.AddWithValue("@Description", description);
    command.Parameters.AddWithValue("@FeaturesTitle", featuresTitle);
    command.Parameters.AddWithValue("@DetailsTitle", detailsTitle);
    command.Parameters.AddWithValue("@PackagesTitle", packagesTitle);
    command.Parameters.AddWithValue("@CreatedAt", now);
    command.Parameters.AddWithValue("@UpdatedAt", now);

    command.ExecuteNonQuery();

    return serviceId;
  }

  private static void InsertServiceFeature(SqliteConnection connection, string serviceId, string text, int order)
  {
    using var command = connection.CreateCommand();
    command.CommandText = @"
        INSERT INTO ServiceFeatures (Id, ServiceId, Text, [Order])
        VALUES (@Id, @ServiceId, @Text, @Order)";

    command.Parameters.AddWithValue("@Id", Nanoid.Generate(size: 12));
    command.Parameters.AddWithValue("@ServiceId", serviceId);
    command.Parameters.AddWithValue("@Text", text);
    command.Parameters.AddWithValue("@Order", order);

    command.ExecuteNonQuery();
  }

  private static void InsertServiceDetail(SqliteConnection connection, string serviceId, string text, int order)
  {
    using var command = connection.CreateCommand();
    command.CommandText = @"
        INSERT INTO ServiceDetails (Id, ServiceId, Text, [Order])
        VALUES (@Id, @ServiceId, @Text, @Order)";

    command.Parameters.AddWithValue("@Id", Nanoid.Generate(size: 12));
    command.Parameters.AddWithValue("@ServiceId", serviceId);
    command.Parameters.AddWithValue("@Text", text);
    command.Parameters.AddWithValue("@Order", order);

    command.ExecuteNonQuery();
  }

  private static string InsertPackage(SqliteConnection connection, string serviceId, string name, string price, bool isPopular, int order)
  {
    var packageId = Nanoid.Generate(size: 12);

    using var command = connection.CreateCommand();
    command.CommandText = @"
        INSERT INTO Packages (Id, ServiceId, Name, Price, IsPopular, [Order], CreatedAt)
        VALUES (@Id, @ServiceId, @Name, @Price, @IsPopular, @Order, @CreatedAt)";

    command.Parameters.AddWithValue("@Id", packageId);
    command.Parameters.AddWithValue("@ServiceId", serviceId);
    command.Parameters.AddWithValue("@Name", name);
    command.Parameters.AddWithValue("@Price", price);
    command.Parameters.AddWithValue("@IsPopular", isPopular ? 1 : 0);
    command.Parameters.AddWithValue("@Order", order);
    command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("o"));

    command.ExecuteNonQuery();

    return packageId;
  }

  private static void SeedSells(SqliteConnection connection)
  {
    // Primeiro, recuperamos os IDs necessários
    var userIds = GetAllUserIds(connection);
    if (userIds.Count == 0) return;

    // Recuperar serviços e seus pacotes
    var services = GetAllServices(connection);
    if (services.Count == 0) return;

    // Inserir vendas
    var random = new Random();
    var statuses = new[] { 0, 1, 2 }; // Pending, Completed, Cancelled

    // Data base para as vendas (2 meses atrás)
    var baseDate = DateTime.UtcNow.AddMonths(-2);

    using var command = connection.CreateCommand();

    // Criar 50 vendas distribuídas ao longo de 2 meses
    for (int i = 0; i < 50; i++)
    {
      var userId = userIds[random.Next(userIds.Count)];
      var service = services[random.Next(services.Count)];

      if (service.Packages.Count == 0) continue;

      var package = service.Packages[random.Next(service.Packages.Count)];
      var status = statuses[random.Next(statuses.Length)];

      // Converter preço de string para decimal (removendo "R$ " e substituindo , por .)
      var priceStr = package.Price.Replace("R$ ", "").Replace(",", ".");
      if (!decimal.TryParse(priceStr, out var price))
      {
        price = random.Next(20, 600);
      }

      // Gerar data aleatória nos últimos 2 meses
      var daysToAdd = random.Next(0, 60); // De 0 a 60 dias
      var createdAt = baseDate.AddDays(daysToAdd);

      // Se status é Completed, adicionar data de conclusão
      DateTime? completedAt = null;
      if (status == 1) // Completed
      {
        completedAt = createdAt.AddHours(random.Next(1, 48)); // Concluído entre 1 e 48 horas depois
      }

      // Inserir a venda
      command.CommandText = @"
          INSERT INTO Sells (Id, UserId, ServiceId, PackageId, Amount, Status, CreatedAt, CompletedAt)
          VALUES (@Id, @UserId, @ServiceId, @PackageId, @Amount, @Status, @CreatedAt, @CompletedAt)";

      command.Parameters.Clear();
      command.Parameters.AddWithValue("@Id", Nanoid.Generate(size: 12));
      command.Parameters.AddWithValue("@UserId", userId);
      command.Parameters.AddWithValue("@ServiceId", service.Id);
      command.Parameters.AddWithValue("@PackageId", package.Id);
      command.Parameters.AddWithValue("@Amount", price);
      command.Parameters.AddWithValue("@Status", status);
      command.Parameters.AddWithValue("@CreatedAt", createdAt.ToString("o"));
      command.Parameters.AddWithValue("@CompletedAt", completedAt.HasValue ? completedAt.Value.ToString("o") : (object)DBNull.Value);

      command.ExecuteNonQuery();
    }
  }

  private static List<string> GetAllUserIds(SqliteConnection connection)
  {
    var ids = new List<string>();

    using var command = connection.CreateCommand();
    command.CommandText = "SELECT Id FROM Users";

    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
      ids.Add(reader.GetString(0));
    }

    return ids;
  }

  private static List<ServiceWithPackages> GetAllServices(SqliteConnection connection)
  {
    var services = new List<ServiceWithPackages>();

    using var command = connection.CreateCommand();
    command.CommandText = "SELECT Id FROM Services";

    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
      string serviceId = reader.GetString(0);
      var packages = GetPackagesForService(connection, serviceId);

      services.Add(new ServiceWithPackages
      {
        Id = serviceId,
        Packages = packages
      });
    }

    return services;
  }

  private static List<PackageInfo> GetPackagesForService(SqliteConnection connection, string serviceId)
  {
    var packages = new List<PackageInfo>();

    using var command = connection.CreateCommand();
    command.CommandText = "SELECT Id, Price FROM Packages WHERE ServiceId = @ServiceId";
    command.Parameters.AddWithValue("@ServiceId", serviceId);

    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
      packages.Add(new PackageInfo
      {
        Id = reader.GetString(0),
        Price = reader.GetString(1)
      });
    }

    return packages;
  }

  private class ServiceWithPackages
  {
    public string Id { get; set; } = string.Empty;
    public List<PackageInfo> Packages { get; set; } = new List<PackageInfo>();
  }

  private class PackageInfo
  {
    public string Id { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
  }
}
