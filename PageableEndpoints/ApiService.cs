public class ApiService
{
    public Pager<Pet> GetAllPetsAsync(PetPageRequest request)
    {
        // TODO: clone request
        var pageable = new OffsetPager<PetPageRequest, PetPageResponse, Pet>(
            request,
            GetPetsPageAsync,
            request => request.Page ?? 0,
            (request, offset) => { request.Page = offset; },
            request => null,
            response => response.Pets,
            response => response.PaginationInfo.HasNextPage
        );
        return pageable;
    }

    private async Task<PetPageResponse> GetPetsPageAsync(PetPageRequest request)
    {
        request.Page = request.Page == null ? 1 : request.Page + 1;
        var response = new PetPageResponse
        {
            Pets =
            [
                new Pet { Name = "Fluffy", Age = 3, Breed = "Golden Retriever", Color = "Yellow", Species = "Dog" },
                new Pet { Name = "Whiskers", Age = 2, Breed = "Siamese", Color = "White", Species = "Cat" },
                new Pet { Name = "Spike", Age = 1, Breed = "Bulldog", Color = "Brown", Species = "Dog" },
                new Pet { Name = "Mittens", Age = 4, Breed = "Tabby", Color = "Gray", Species = "Cat" },
                new Pet { Name = "Fido", Age = 5, Breed = "Dalmatian", Color = "Black", Species = "Dog" },
            ],
            PaginationInfo = new PaginationInfo
            {
                HasNextPage = request.Page <= 5
            }
        };
        await Task.Delay(200);
        return response;
    }

    public Pager<Pet> GetAllPetsCursorAsync(PetCursorRequest request)
    {
        // TODO: clone request
        var pageable = new CursorPager<PetCursorRequest, PetCursorResponse, Pet>(
            request,
            GetPetsCursorAsync,
            (request, cursor) => { request.Cursor = cursor; },
            response => response.CursorInfo.Next,
            response => response.Pets
        );
        return pageable;
    }

    private async Task<PetCursorResponse> GetPetsCursorAsync(PetCursorRequest request)
    {
        var response = new PetCursorResponse
        {
            Pets =
            [
                new Pet { Name = "Fluffy", Age = 3, Breed = "Golden Retriever", Color = "Yellow", Species = "Dog" },
                new Pet { Name = "Whiskers", Age = 2, Breed = "Siamese", Color = "White", Species = "Cat" },
                new Pet { Name = "Spike", Age = 1, Breed = "Bulldog", Color = "Brown", Species = "Dog" },
                new Pet { Name = "Mittens", Age = 4, Breed = "Tabby", Color = "Gray", Species = "Cat" },
                new Pet { Name = "Fido", Age = 5, Breed = "Dalmatian", Color = "Black", Species = "Dog" },
            ],
            CursorInfo = new CursorInfo()
            {
                Next = request.Cursor == null ? "1" : int.Parse(request.Cursor) + 1 + ""
            }
        };
        if (response.CursorInfo.Next == "5") response.CursorInfo.Next = null;
        await Task.Delay(200);
        return response;
    }
}

public class PetPageRequest
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}

public class PetPageResponse
{
    public List<Pet> Pets { get; set; }
    public PaginationInfo PaginationInfo { get; set; }
}

public class PaginationInfo
{
    public bool HasNextPage { get; set; }
}

public class PetCursorRequest
{
    public string? Cursor { get; set; }
}

public class PetCursorResponse
{
    public List<Pet> Pets { get; set; }
    public CursorInfo CursorInfo { get; set; }
}

public class CursorInfo
{
    public string? Next { get; set; }
}

public class Pet
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Breed { get; set; }
    public string Color { get; set; }
    public string Species { get; set; }
}