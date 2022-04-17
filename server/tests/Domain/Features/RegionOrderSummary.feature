Feature: Region order summary

Background:
    Given region 'The Forge'
    And item type '1' - 'Item 1'
    And order summary:
    | ItemTypeId | ItemTypeName | Price | VolumeRemaining | IsValid | ExpirationDateTime   |
    | 1          | Item 1       | 0.10  | 1000000         | true    | 2500-01-01T:00:00:00 |
    
Scenario: Do not refresh valid order summaries
    When refreshing order summary for item 'Item 1' and a volume of '10'
    Then refresh aborted because summary is still valid 'true'