const Sequelize = require('sequelize');
module.exports = function(sequelize, DataTypes) {
  return sequelize.define('Cities', {
    City_Id: {
      autoIncrement: true,
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true
    },
    City_name: {
      type: DataTypes.STRING(450),
      allowNull: false,
      defaultValue: "(N"
    }
  }, {
    sequelize,
    tableName: 'Cities',
    schema: 'dbo',
    timestamps: false,
    indexes: [
      {
        name: "IX_Cities_City_name",
        unique: true,
        fields: [
          { name: "City_name" },
        ]
      },
      {
        name: "PK_Cities",
        unique: true,
        fields: [
          { name: "City_Id" },
        ]
      },
    ]
  });
};
