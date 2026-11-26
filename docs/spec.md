# Economy Simulator

Codename: Bizio

# Description

As the player, you take ownership of a company that buys, crafts, and sells resources in a global economy with other NPC players. Your goal is to stay alive, stay relevant, and thrive.

# Main Mechanics

- The game happens in turns, not real time
- Customer
  - Purchases products
  - Does not purchase resources
  - Desires a variety of attributes, not particular products
    - Their desire for attributes A, B, and C might justify a purchase of product X because of its "best fit" for those attributes
  - Desires change over time
- Resource
  - A primary component in manufacturing
  - Attributes
    - Describe the quality or utility of the resource
    - Allows multiple producers of the same resource to have different variations of it
- Manufacturing
  - The process of applying one or more operations to one or more resources or products
  - Types of operations
    - Heat
    - Cool
    - Combine
    - Distill
    - Crush
    - Expand
  - Each operation has an energy cost
    - Energy is an under-developed concept
    - For now, let's assume energy has a fixed cost across all players for the entire game
  - Operations consume input resources/products and product output products
  - Output products have new attributes resulting from the operation applied to the inputs
- Product
  - The result of manufacturing
  - Products may expire or degrade
- Companies
  - Purchase resources or products
  - Manufacture products
    - Pipelines
      - May have up to 3 pipelines
      - Each pipeline may have up to 3 stages
      - Each pipeline is run completely each turn
      - Each pipeline stage defines the inputs and outputs
      - Players may "scale up" a pipeline
        - For example, if a pipeline takes 1 sugar and 1 water to create 1 syrup, then the player can specify that each turn they will provide 100x, meaning it will require 100 sugar and 100 water to make 100 syrup
  - Sell products to customers
  - Sell products or resources to companies
- Producers
  - Make primary resources (like wood, water, fruit, etc)
  - Non-competitive, non-emergent, unlike the NPC companies rivaling the player
  - May change their prices, may produce at different rates, or be subject to other variable effects
  - Intentionally do not participate in manufacturing or influencing the economy on purpose
