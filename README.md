# P2FixAnAppDotNetCode — Rapport de correction

## Description du projet

Application web ASP.NET Core (net 9.0) de type boutique en ligne ("OpenClassrooms shop") permettant de :
- Afficher une liste de produits
- Ajouter des produits au panier
- Passer une commande
- Changer la langue de l'interface (Anglais, Français, Espagnol)

---

## ✅ Résultat final

| Indicateur | Avant | Après |
|---|---|---|
| Compilation | ✅ 0 erreur | ✅ 0 erreur |
| Tests unitaires | ❌ 1/8 réussi | ✅ **8/8 réussis** |
| Panier (AddItem) | ❌ Ne fonctionne pas | ✅ Fonctionne |
| Panier (Total/Moyenne) | ❌ Toujours 0 | ✅ Calculé correctement |
| Ajout au panier (bouton) | ❌ Aucun effet | ✅ Fonctionne |
| Décrément du stock | ❌ Jamais décrémenté | ✅ Fonctionne |
| Changement de langue | ❌ Aucun effet | ✅ Fonctionne |

---

## 🐛 Bugs corrigés et TODOs implémentés

### 1. `Models/Cart.cs` — Cause racine + 4 méthodes

**Problème racine :** La méthode `GetCartLineList()` retournait `new List<CartLine>()` à chaque appel — une nouvelle liste vide à chaque fois. Aucun produit n'était donc jamais conservé dans le panier.

**Correction :** Ajout d'un champ privé `_cartLines` persistant pour toute la durée de vie de l'instance :
```csharp
// AVANT (bug) :
private List<CartLine> GetCartLineList()
{
    return new List<CartLine>(); // ← nouvelle liste vide à chaque appel !
}

// APRÈS (corrigé) :
private readonly List<CartLine> _cartLines = new List<CartLine>();
private List<CartLine> GetCartLineList() => _cartLines;
```

**TODOs implémentés :**

#### `AddItem(Product product, int quantity)`
Ajoute un produit au panier. Si le produit est déjà présent (même `Id`), sa quantité est incrémentée ; sinon, une nouvelle ligne est créée.
```csharp
CartLine existingLine = _cartLines.FirstOrDefault(l => l.Product.Id == product.Id);
if (existingLine == null)
    _cartLines.Add(new CartLine { ... });
else
    existingLine.Quantity += quantity;
```

#### `GetTotalValue()`
Somme de `prix × quantité` pour chaque ligne du panier.
```csharp
return _cartLines.Sum(l => l.Product.Price * l.Quantity);
```

#### `GetAverageValue()`
Valeur totale divisée par le nombre total d'articles.
```csharp
return GetTotalValue() / _cartLines.Sum(l => l.Quantity);
```

#### `FindProductInCartLines(int productId)`
Recherche un produit dans les lignes du panier par son identifiant.
```csharp
return _cartLines.FirstOrDefault(l => l.Product.Id == productId)?.Product;
```

---

### 2. `Models/Services/ProductService.cs` — 2 méthodes

#### `GetProductById(int id)`
Recherche un produit dans l'inventaire par son identifiant via le repository.
```csharp
return _productRepository.GetAllProducts().FirstOrDefault(p => p.Id == id);
```
> **Impact bug :** Sans cette implémentation, `CartController.AddToCart()` retournait toujours `null`, rendant le bouton "Ajouter au panier" totalement non fonctionnel.

#### `UpdateProductQuantities(Cart cart)`
Parcourt les lignes du panier et décrémente le stock de chaque produit commandé.
```csharp
foreach (CartLine line in cart.Lines)
{
    _productRepository.UpdateProductStocks(line.Product.Id, line.Quantity);
}
```

---

### 3. `Models/Services/LanguageService.cs` — 1 méthode

#### `SetCulture(string language)`
Mappe le nom de la langue sélectionnée vers le code de culture correspondant.
```csharp
switch (language)
{
    case "French":  return "fr";
    case "Spanish": return "es";
    default:        return "en"; // Anglais par défaut
}
```

---

### 4. `Models/Repositories/ProductRepository.cs` — Correctif constructeur

**Problème :** Le champ `_products` est `static` (partagé entre toutes les instances), mais le constructeur le réinitialisait à chaque `new ProductRepository()`, effaçant les modifications de stock déjà effectuées.

**Correction :** Ajout d'une vérification `null` pour n'initialiser les données qu'une seule fois :
```csharp
// AVANT (bug) :
public ProductRepository()
{
    _products = new List<Product>(); // ← réinitialise le stock à chaque fois !
    GenerateProductData();
}

// APRÈS (corrigé) :
public ProductRepository()
{
    if (_products == null) // n'initialise que si pas encore fait
    {
        _products = new List<Product>();
        GenerateProductData();
    }
}
```
> **Impact :** Le test `UpdateProductQuantities` vérifie que le stock diminue correctement sur plusieurs passages de commande. Sans ce correctif, le stock était remis à zéro entre chaque appel.

---

## 🏗️ Structure du projet

```
P2FixAnAppDotNetCode/
├── Program.cs                        ← Point d'entrée (ne pas modifier)
├── Startup.cs                        ← Injection de dépendances + localisation
├── Controllers/
│   ├── CartController.cs             ← Gestion du panier (ne pas modifier)
│   ├── OrderController.cs            ← Gestion des commandes (ne pas modifier)
│   ├── ProductController.cs          ← Affichage des produits (ne pas modifier)
│   └── LanguageController.cs         ← Changement de langue (ne pas modifier)
├── Models/
│   ├── Cart.cs                       ← ✏️ MODIFIÉ
│   ├── ICart.cs                      ← Interface (ne pas modifier)
│   ├── Product.cs                    ← Modèle de données (ne pas modifier)
│   ├── Order.cs                      ← Modèle de données (ne pas modifier)
│   ├── Services/
│   │   ├── ProductService.cs         ← ✏️ MODIFIÉ
│   │   ├── LanguageService.cs        ← ✏️ MODIFIÉ
│   │   └── OrderService.cs           ← Déjà fonctionnel (ne pas modifier)
│   └── Repositories/
│       ├── ProductRepository.cs      ← ✏️ MODIFIÉ (correctif constructeur)
│       └── OrderRepository.cs        ← (ne pas modifier)
├── Views/                            ← Toutes les vues (ne pas modifier)
└── Resources/                        ← Fichiers de traduction .resx (ne pas modifier)

P2FixAnAppDotNetCode.Tests/
├── CartTests.cs                      ← 4 tests (tous réussis ✅)
├── ProductServiceTests.cs            ← 3 tests (tous réussis ✅)
└── LanguageServiceTests.cs           ← 1 test (réussi ✅)
```

---

## 🧪 Résultats des tests unitaires

```
Série de tests réussie.
Nombre total de tests : 8
     Réussi(s) : 8
Durée totale : 1,456 Secondes
```

| Test | Classe | Statut |
|---|---|---|
| `AddItemInCart` | CartTests | ✅ Réussi |
| `GetTotalValue` | CartTests | ✅ Réussi |
| `GetAverageValue` | CartTests | ✅ Réussi |
| `FindProductInCartLines` | CartTests | ✅ Réussi |
| `Product` (type retour) | ProductServiceTests | ✅ Réussi |
| `GetProductById` | ProductServiceTests | ✅ Réussi |
| `UpdateProductQuantities` | ProductServiceTests | ✅ Réussi |
| `SetCulture` | LanguageServiceTests | ✅ Réussi |

---

## 🔧 Comment lancer le projet

```bash
# Compiler
dotnet build P2FixAnAppDotNetCode.sln

# Lancer les tests
dotnet test P2FixAnAppDotNetCode.Tests/P2FixAnAppDotNetCode.Tests.csproj

# Démarrer l'application web
cd P2FixAnAppDotNetCode
dotnet run
```

L'application sera accessible sur `http://localhost:5000` (ou le port indiqué dans la console).