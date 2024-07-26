type Undefined = exn

type AsyncResult<'success, 'failure> = Async<Result<'success, 'failure>>
