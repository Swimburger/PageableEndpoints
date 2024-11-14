namespace PageableEndpoints;

public class Tests
{
    [Test]
    public async Task TestPagination()
    {
        var apiService = new ApiService();
        var pets = apiService.GetAllPetsAsync(new PetPageRequest
        {
            Page = 1
        });
        var petList = new List<Pet>();
        await foreach (var pet in pets)
        {
            petList.Add(pet);
        }

        Assert.That(petList, Has.Count.EqualTo(25));
    }

    [Test]
    public async Task TestGetPaginationResponse()
    {
        var apiService = new ApiService();
        var pages = apiService.GetAllPetsAsync(new PetPageRequest
        {
            Page = 1
        }).AsPagesAsync();
        var index = 1;
        await foreach(var page in pages)
        {
            Assert.Multiple(() =>
            {
                Assert.That(page.Items, Has.Count.EqualTo(5));
            });
            index++;
        }
    }
    
    [Test]
    public async Task TestCursor()
    {
        var apiService = new ApiService();
        var pets = apiService.GetAllPetsCursorAsync(new PetCursorRequest());
        var petList = new List<Pet>();
        await foreach (var pet in pets)
        {
            petList.Add(pet);
        }

        Assert.That(petList, Has.Count.EqualTo(25));
    }

    [Test]
    public async Task TestGetCursorResponse()
    {
        var apiService = new ApiService();
        var pages = apiService.GetAllPetsCursorAsync(new PetCursorRequest()).AsPagesAsync();
        var index = 1;
        await foreach(var page in pages)
        {
            Assert.Multiple(() =>
            {
                Assert.That(page.Items, Has.Count.EqualTo(5));
            });
            index++;
        }
    }
}

