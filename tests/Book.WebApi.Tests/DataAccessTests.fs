module DataAccessTests

open Xunit
open Book.WebApi.DataAccess
open Book.WebApi.Models
open Fixtures
open System.Linq;

[<Fact>]
let ``getAll should return correct result``() = 
    let entity = { getTestBook with BookId = nextId() }
    //Arrange
    let context = initializeAndPopulateContext entity
    //Act
    let count = getAll context |> Seq.toList |> List.length
    Assert.True(count > 0)

[<Fact>]
let ``getAll should not return empty result``() = 
    let entity = { getTestBook with BookId = nextId() }
    //Arrange
    let context = initializeAndPopulateContext entity
    let result = getAll context
    //Act
    result |> shouldNotNull
    let count =  result.Count()
    Assert.True(count > 0)

[<Fact>]
let ``getBook should return correct result ``() = 
    let currentId = nextId()
    let entity = { getTestBook with BookId = currentId }
    //Arrange
    let context = initializeAndPopulateContext entity
    let first = context.Books.FirstOrDefault(fun x-> x.BookId = if currentId = 0 then 1 else currentId)
    //Act
    let result = getBook context first.BookId
    //Assert
    Assert.Equal(result.Value.BookId, first.BookId)

[<Fact>]
let ``addBookAsync should return inserted result ``() = 
    async {
        let currentId = nextId()
        let entity = { getTestBook with BookId = currentId }
    //Arrange
        let context = initializeAndPopulateContext entity
        //Arrange
        //let context = initializeInMemoryContext
        let entity = { getTestBook with BookId = nextId() }
        //Act
        let! result = addBookAsync context entity
        //Assert
        result |> shouldNotNull
    }

[<Fact>]
let ``updateBook should update correct record ``() = 
    let currentId = nextId()
    let entity = { getTestBook with BookId = currentId }
    //Arrange
    let context = initializeAndPopulateContext entity

    let first = context.Books.FirstOrDefault(fun x-> x.BookId = if currentId = 0 then 1 else currentId)

    //Act
    let result = 
        updateBook context {  BookId = first.BookId
                              Author = "Test" 
                              Title = "Test" 
                              PublishedYear = 2018
                              Pages = 465  } first.BookId
    //Assert
    result.Value.Author |> shouldEqual "Test"

[<Fact>]
let ``deleteBook should delete correct record ``() =
    let currentId = nextId()
    let entity = { getTestBook with BookId = currentId }
    //Arrange
    let context = initializeAndPopulateContext entity

    let first = context.Books.FirstOrDefault(fun x-> x.BookId = if currentId = 0 then 1 else currentId)
    //Act
    let result = deleteBook context first.BookId
    //Assert
    Assert.Equal(result.Value.BookId, first.BookId)
    
