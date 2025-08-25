
# [Coalesce]

`IntelliTect.Coalesce.CoalesceAttribute`

Used to mark a type or member for generation by Coalesce.

Some types and members will be implicitly included in generation - for example, all types represented by a `DbSet<T>` on a `DbContext` that has a `[Coalesce]` attribute will automatically be included. Properties on these types will also be generated for unless explicitly excluded, since this is by far the most common usage scenario in Coalesce.

On the other hand, [Methods](/modeling/model-components/methods.md) on these types will not have endpoints generated unless they are explicitly annotated with `[Coalesce]` to avoid accidentally exposing methods that were perhaps not intended to be exposed.

## Type-Level Usage

When used on a class, the `[Coalesce]` attribute will cause the type to be included in Coalesce generation.

The documentation pages for types and members that require/accept this attribute will state as such. An exhaustive list of all valid targets for `[Coalesce]` will not be found on this page.
