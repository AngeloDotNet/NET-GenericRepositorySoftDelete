# Template-WebApp

Note implementative e suggerimenti

- Il sorting dinamico usa EF.Property<object>(e, propertyName). Valida che i nomi delle proprietà passate in sortBy siano corretti per evitare runtime errors; puoi aggiungere una whitelist o reflection-based validation se vuoi sicurezza.
- Il global query filter considera qualsiasi entità che implementa ISoftDelete. Le entità non soft-deletable restano inalterate.
- Per "undelete" (restore) basta recuperare l'entità con IgnoreQueryFilters(), impostare IsDeleted = false e salvare.
- Se preferisci usare System.Linq.Dynamic.Core per sorting dinamico con stringhe (supporta "Name desc"), posso sostituire l'implementazione con quella libreria.