AsIs Classes:
* `Page`: Holds a single page of items and the response mapped to a type object
* `Pageable`: The class the user interacts with to iterate through items or pages
* `OffsetPageable`: An abstract class containing the logic for offset-based pagination, extends `Pageable`
* `CursorPageable`: An abstract class containing the logic for cursor-based pagination, extends `Pageable`

Generated Classes (manually written for now):
* `PetPageRequestPageResponsePageable`: A class that extends `OffsetPageable` and implements the abstract methods to get relevant data from the request and response.
* `PetCursorRequestPetCursorResponsePageable`: A class that extends `CursorPageable` and implements the abstract methods to get relevant data from the request and response.
* `PetPageRequest`
* `PetPageResponse`
* `PetCursorRequest`
* `PetCursorResponse`
* `ApiService`